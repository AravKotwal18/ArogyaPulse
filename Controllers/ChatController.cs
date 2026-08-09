using Microsoft.AspNetCore.Mvc;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IAiChatService _chatService;

        public ChatController(IAiChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            string queryText = !string.IsNullOrWhiteSpace(request.Message) ? request.Message : request.Query;
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return BadRequest(new { success = false, message = "Query or message cannot be empty." });
            }

            var result = await _chatService.GetGuidanceAsync(request);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }
}
