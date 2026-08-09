using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArogyaPulse.Api.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AppDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(int? patientId, string action, string performedBy, string details)
        {
            var entry = new AuditLog
            {
                PatientId = patientId,
                Action = action,
                PerformedBy = performedBy,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "[Audit] {Action} on Patient #{PatientId} by {PerformedBy}: {Details}",
                action, patientId, performedBy, details);
        }

        public async Task<List<AuditLog>> GetByPatientIdAsync(int patientId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentAsync(int count = 50)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
