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

        /// <summary>
        /// Records captured within this window (minutes) for the same patient
        /// name + village are treated as potential duplicates.
        /// </summary>
        private const int DuplicateWindowMinutes = 30;

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

        public async Task<SyncResponseDto> ProcessBatchAsync(SyncRequestDto request)
        {
            var response = new SyncResponseDto
            {
                TotalReceived = request.Records.Count
            };

            foreach (var record in request.Records)
            {
                var localId = string.IsNullOrWhiteSpace(record.LocalRecordId) ? Guid.NewGuid().ToString() : record.LocalRecordId;
                var result = new SyncRecordResultDto
                {
                    LocalRecordId = localId,
                    PatientName = record.Name
                };

                try
                {
                    // 1. Idempotency Check: Already synced by LocalRecordId UUID
                    var existingByLocalId = await _context.Patients.FirstOrDefaultAsync(p => p.LocalRecordId == localId);
                    if (existingByLocalId != null)
                    {
                        result.Status = "Synced";
                        result.PatientId = existingByLocalId.Id;
                        result.Message = $"Already synced (Idempotent UUID match #{existingByLocalId.Id}).";
                        response.Synced++;
                        response.Results.Add(result);
                        continue;
                    }

                    // 2. Duplicate/Conflict Check: Name + Village match within time window
                    var windowStart = record.CapturedAt.AddMinutes(-DuplicateWindowMinutes);
                    var windowEnd = record.CapturedAt.AddMinutes(DuplicateWindowMinutes);

                    var isDuplicate = await _context.Patients.AnyAsync(p =>
                        p.Name.ToLower() == record.Name.ToLower() &&
                        p.Village.ToLower() == record.Village.ToLower() &&
                        p.Timestamp >= windowStart &&
                        p.Timestamp <= windowEnd);

                    if (isDuplicate)
                    {
                        result.Status = "Conflict";
                        result.Message = $"Duplicate conflict: patient '{record.Name}' in '{record.Village}' captured within {DuplicateWindowMinutes}-minute window.";
                        response.Conflicts++;

                        _context.SyncLogs.Add(new SyncLog
                        {
                            DeviceId = request.DeviceId,
                            Action = "SyncConflict",
                            LocalRecordId = localId,
                            Payload = JsonSerializer.Serialize(record),
                            Status = "Conflict",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        // 3. Create new patient from offline record
                        var gender = string.IsNullOrWhiteSpace(record.Gender) ? "Unknown" : record.Gender;
                        var patient = new Patient
                        {
                            Name = record.Name,
                            Age = record.Age,
                            Gender = gender,
                            BloodGroup = string.IsNullOrWhiteSpace(record.BloodGroup) ? "Unknown" : record.BloodGroup,
                            Village = record.Village,
                            Bp = record.Vitals.Bp,
                            SpO2 = record.Vitals.SpO2,
                            Temp = record.Vitals.Temp,
                            Glucose = record.Vitals.Glucose,
                            Symptoms = record.Symptoms,
                            IsPregnant = gender == "Female" ? record.IsPregnant : false,
                            Status = "Pending",
                            LocalRecordId = localId,
                            Timestamp = record.CapturedAt
                        };

                        // Run triage
                        var triage = _triageService.EvaluateTriage(
                            patient.Bp, patient.SpO2, patient.Temp, patient.Glucose, patient.IsPregnant);
                        patient.RiskScore = triage.TotalScore;
                        patient.RiskLevel = triage.RiskLevel;

                        _context.Patients.Add(patient);
                        await _context.SaveChangesAsync();

                        result.Status = "Synced";
                        result.PatientId = patient.Id;
                        result.Message = $"Successfully synced offline record. Risk: {triage.RiskLevel} ({triage.TotalScore}/100)";
                        response.Synced++;

                        // Log sync
                        _context.SyncLogs.Add(new SyncLog
                        {
                            DeviceId = request.DeviceId,
                            Action = "SyncCreated",
                            LocalRecordId = localId,
                            Payload = JsonSerializer.Serialize(record),
                            Status = "Synced",
                            PatientId = patient.Id,
                            CreatedAt = DateTime.UtcNow,
                            SyncedAt = DateTime.UtcNow
                        });

                        await _auditService.LogAsync(
                            patient.Id,
                            "OfflineSync",
                            $"Device:{request.DeviceId}",
                            $"Offline record synced [UUID: {localId}]. Risk: {triage.RiskLevel}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync record for {Name} [UUID: {LocalId}]", record.Name, localId);
                    result.Status = "Error";
                    result.Message = $"Sync failed: {ex.Message}";
                }

                response.Results.Add(result);
            }

            await _context.SaveChangesAsync();
            return response;
        }

        public async Task<List<SyncLog>> GetDeviceHistoryAsync(string deviceId)
        {
            return await _context.SyncLogs
                .AsNoTracking()
                .Where(s => s.DeviceId == deviceId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(100)
                .ToListAsync();
        }
    }
}
