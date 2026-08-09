using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;

namespace ArogyaPulse.Api.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            int patientId,
            string action,
            string performedBy,
            string details)
        {
            var auditLog = new AuditLog
            {
                PatientId = patientId,
                Action = action,
                PerformedBy = performedBy,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetByPatientIdAsync(
            int patientId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentAsync(
            int limit = 100)
        {
            limit = Math.Clamp(limit, 1, 500);

            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
    }
}