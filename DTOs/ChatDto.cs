using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.DTOs
{
    public class ChatRequestDto
    {
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Alias for Query to support both 'message' and 'query' JSON fields.
        /// </summary>
        public string Message
        {
            get => string.IsNullOrWhiteSpace(_message) ? Query : _message;
            set => _message = value;
        }
        private string _message = string.Empty;

        public string Language { get; set; } = "auto";

        /// <summary>
        /// Optional patient ID for contextual queries.
        /// </summary>
        public int? PatientId { get; set; }
    }

    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public List<string> ClinicalAlerts { get; set; } = new();
        public List<string> ActionSteps { get; set; } = new();
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public string LanguageDetected { get; set; } = "English"; // English, Hindi, Tamil, Hinglish
        public string? PatientContext { get; set; }
        public VitalsDto? ExtractedVitals { get; set; }
        public List<string> ExtractedSymptoms { get; set; } = new();
        public TriageResultDto? TriageEvaluation { get; set; }
        public string Disclaimer { get; set; } = "This assistant provides screening support and does not diagnose disease.";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
