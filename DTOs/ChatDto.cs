using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.DTOs
{
    public class ChatRequestDto
    {
        [Required]
        public string Query { get; set; } = string.Empty;
        public string Language { get; set; } = "en"; // "en" or "hi"

        /// <summary>
        /// Optional patient ID for contextual queries. When provided,
        /// the chat service will include patient-specific vital signs context.
        /// </summary>
        public int? PatientId { get; set; }
    }

    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public List<string> ClinicalAlerts { get; set; } = new();
        public List<string> ActionSteps { get; set; } = new();
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public string? PatientContext { get; set; }
        public string Disclaimer { get; set; } = "This guidance is a screening aid based on WHO/NHM protocols. It does not constitute a medical diagnosis. All clinical decisions must be made by a qualified medical professional.";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
