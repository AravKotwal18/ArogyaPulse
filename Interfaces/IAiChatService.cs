using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Interfaces
{
    public interface IAiChatService
    {
        Task<ChatResponseDto> GetGuidanceAsync(ChatDto request);
    }
}