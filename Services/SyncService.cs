using System.Text.Json;
using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.DTOs;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArogyaPulse.Api.Services
{
    public class SyncService : ISyncService
    {
        private readonly AppDbContext _context;
        private readonly ITriageService _triageService;
        private readonly IAuditService _auditService;
        private readonly ILogger<SyncService> _logger;

        public SyncService(
            AppDbContext context,
            ITriageService triageService,
            IAuditService auditService,
            ILogger<SyncService> logger)
        {
            _context = context;
            _triageService = triageService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<SyncResponseDto> ProcessBatchAsync(
            SyncRequestDto request)
        {
            var result = new SyncResponseDto
            {
                TotalReceived = request.Records.Count
            };

            foreach (var record in request.Records)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(
                            record.LocalRecordId))
                    {
                        result.Errors++;

                        result.Results.Add(
                            new SyncRecordResultDto
                            {
                                Status = "Error",
                                PatientName = record.Name,
                                Message = "LocalRecordId is required."
                            });

                        continue;
                    }

                    var existingSync =
                        await _context.SyncLogs
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x =>
                                x.DeviceId ==
                                    request.DeviceId &&
                                x.LocalRecordId ==
                                    record.LocalRecordId);

                    if (existingSync != null)
                    {
                        if (existingSync.Status == "Synced")
                        {
                            result.AlreadySynced++;

                            result.Results.Add(
                                new SyncRecordResultDto
                                {
                                    LocalRecordId =
                                        record.LocalRecordId,

                                    PatientName =
                                        record.Name,

                                    PatientId =
                                        existingSync.PatientId,

                                    Status =
                                        "AlreadySynced",

                                    Message =
                                        "Record was already synchronized."
                                });

                            continue;
                        }

                        if (existingSync.Status == "Conflict")
                        {
                            result.Conflicts++;

                            result.Results.Add(
                                new SyncRecordResultDto
                                {
                                    LocalRecordId =
                                        record.LocalRecordId,

                                    PatientName =
                                        record.Name,

                                    Status = "Conflict",

                                    Message =
                                        "This record previously resulted in a conflict."
                                });

                            continue;
                        }
                    }

                    var patient = new Patient
                    {
                        Name = record.Name.Trim(),

                        Age = record.Age,

                        Gender =
                            string.IsNullOrWhiteSpace(record.Gender)
                                ? "Unknown"
                                : record.Gender.Trim(),

                        BloodGroup =
                            string.IsNullOrWhiteSpace(record.BloodGroup)
                                ? "Unknown"
                                : record.BloodGroup.Trim(),

                        Village = record.Village.Trim(),

                        Bp = record.Vitals.Bp.Trim(),

                        SpO2 = record.Vitals.SpO2,

                        Temp = record.Vitals.Temp,

                        Glucose = record.Vitals.Glucose,

                        Symptoms =
                            record.Symptoms?.Trim() ?? string.Empty,

                        IsPregnant =
                            record.IsPregnant,

                        Status = "Pending",

                        Timestamp = record.CapturedAt
                    };

                    var triage =
                        _triageService.EvaluateTriage(
                            patient.Bp,
                            patient.SpO2,
                            patient.Temp,
                            patient.Glucose,
                            patient.IsPregnant);

                    patient.RiskScore =
                        triage.TotalScore;

                    patient.RiskLevel =
                        triage.RiskLevel;

                    _context.Patients.Add(patient);

                    await _context.SaveChangesAsync();

                    var syncLog = new SyncLog
                    {
                        DeviceId = request.DeviceId,

                        LocalRecordId =
                            record.LocalRecordId,

                        Action = "SyncCreated",

                        Payload =
                            JsonSerializer.Serialize(record),

                        Status = "Synced",

                        PatientId = patient.Id,

                        CreatedAt = DateTime.UtcNow,

                        SyncedAt = DateTime.UtcNow
                    };

                    _context.SyncLogs.Add(syncLog);

                    await _context.SaveChangesAsync();

                    await _auditService.LogAsync(
                        patient.Id,
                        "OfflineSync",
                        $"Device:{request.DeviceId}",
                        $"Offline record {record.LocalRecordId} synchronized. Risk={triage.RiskLevel}.");

                    result.Synced++;

                    result.Results.Add(
                        new SyncRecordResultDto
                        {
                            LocalRecordId =
                                record.LocalRecordId,

                            PatientName =
                                patient.Name,

                            PatientId =
                                patient.Id,

                            Status = "Synced",

                            Message =
                                $"Successfully synchronized. Risk={triage.RiskLevel}."
                        });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(
                        ex,
                        "Database error during sync for {LocalRecordId}",
                        record.LocalRecordId);

                    result.Errors++;

                    result.Results.Add(
                        new SyncRecordResultDto
                        {
                            LocalRecordId =
                                record.LocalRecordId,

                            PatientName =
                                record.Name,

                            Status = "Error",

                            Message =
                                "The record could not be synchronized."
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected sync error.");

                    result.Errors++;

                    result.Results.Add(
                        new SyncRecordResultDto
                        {
                            LocalRecordId =
                                record.LocalRecordId,

                            PatientName =
                                record.Name,

                            Status = "Error",

                            Message =
                                "Unexpected synchronization error."
                        });
                }
            }

            return result;
        }

        public async Task<List<SyncLog>>
            GetDeviceHistoryAsync(
                string deviceId)
        {
            return await _context.SyncLogs
                .AsNoTracking()
                .Where(x => x.DeviceId == deviceId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .ToListAsync();
        }
    }
}