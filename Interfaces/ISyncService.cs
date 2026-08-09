using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Interfaces
{
    public interface ISyncService
    {
        Task<SyncResponseDto> ProcessBatchAsync(SyncRequestDto request);
        Task<List<Models.SyncLog>> GetDeviceHistoryAsync(string deviceId);
    }
}
