using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;

namespace ArogyaPulse.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendHighRiskAlertAsync(
            Patient patient)
        {
            _logger.LogWarning(
                "[Alert Stub] High-risk patient #{PatientId} " +
                "requires doctor review. Risk={RiskLevel}, Score={Score}",
                patient.Id,
                patient.RiskLevel,
                patient.RiskScore);

            return Task.FromResult(true);
        }
    }
}