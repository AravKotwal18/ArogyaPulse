using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.DTOs
{
    public class ChatRequestDto
    {
        [Required]
        public string Query { get; set; } = string.Empty;
        public string Language { get; set; } = "en"; // "en" or "hi"
    }

    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public List<string> ClinicalAlerts { get; set; } = new();
        public List<string> ActionSteps { get; set; } = new();
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
