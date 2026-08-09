using ArogyaPulse.Api.Models;
namespace ArogyaPulse.Api.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendHighRiskAlertAsync(Patient patient);
    }
}