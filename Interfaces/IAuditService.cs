namespace ArogyaPulse.Api.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(int? patientId, string action, string performedBy, string details);
        Task<List<Models.AuditLog>> GetByPatientIdAsync(int patientId);
        Task<List<Models.AuditLog>> GetRecentAsync(int count = 50);
    }
}
