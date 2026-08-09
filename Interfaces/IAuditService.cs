using ArogyaPulse.Api.Models;

namespace ArogyaPulse.Api.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            int patientId,
            string action,
            string performedBy,
            string details);

        Task<List<AuditLog>> GetByPatientIdAsync(
            int patientId);

        Task<List<AuditLog>> GetRecentAsync(
            int limit = 100);
    }
}