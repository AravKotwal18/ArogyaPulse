using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;

namespace ArogyaPulse.Api.Services
{
    /// <summary>
    /// Logging-only alert stub. In production, this would integrate with
    /// an SMS gateway (e.g., Twilio) or messaging API. Currently logs
    /// the alert message for demonstration purposes.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendHighRiskAlertAsync(Patient patient)
        {
            var message =
                $"🚨 ArogyaPulse Alert 🚨\n\n" +
                $"High Risk Patient Detected\n" +
                $"Name: {patient.Name}\n" +
                $"Village: {patient.Village}\n" +
                $"Risk Level: {patient.RiskLevel}\n" +
                $"Risk Score: {patient.RiskScore}/100\n\n" +
                $"Please review immediately on the Doctor Dashboard.";

            _logger.LogInformation("[Alert Service] High-risk alert triggered for patient #{Id}", patient.Id);
            _logger.LogInformation("[Alert Content]:\n{Message}", message);

            return Task.FromResult(true);
        }
    }
}