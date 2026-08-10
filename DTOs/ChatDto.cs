using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.DTOs
{
    public class ChatDto
    {
        [MaxLength(
            1000,
            ErrorMessage = "Message cannot exceed 1000 characters")]
        public string? Message { get; set; }

        [MaxLength(
            1000,
            ErrorMessage = "Query cannot exceed 1000 characters")]
        public string? Query { get; set; }

        /// <summary>
        /// Optional patient ID used to provide
        /// screening context to the AI assistant.
        /// </summary>
        public int? PatientId { get; set; }
    }

    public class ExtractedVitalsDto
    {
        public string? Bp { get; set; }
        public int SpO2 { get; set; }
        public double Temp { get; set; }
        public int Glucose { get; set; }
    }

    public class ChatResponseDto
    {
        /// <summary>
        /// Main assistant response shown to the ASHA worker.
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Detected language of the input query (e.g. English, Hindi).
        /// </summary>
        public string LanguageDetected { get; set; } = "English";

        /// <summary>
        /// Extracted clinical vital signs from query or patient record.
        /// </summary>
        public ExtractedVitalsDto? ExtractedVitals { get; set; }

        /// <summary>
        /// Extracted clinical symptoms.
        /// </summary>
        public List<string> ExtractedSymptoms { get; set; } = new();

        /// <summary>
        /// Automated triage evaluation result based on extracted vitals.
        /// </summary>
        public TriageResultDto? TriageEvaluation { get; set; }

        /// <summary>
        /// AI-generated clinical alerts that should
        /// be verified by a healthcare professional.
        /// </summary>
        public List<string> ClinicalAlerts { get; set; } = new();

        /// <summary>
        /// Additional information the ASHA worker
        /// should collect.
        /// </summary>
        public List<string> ActionSteps { get; set; } = new();

        /// <summary>
        /// Informational severity of the AI response.
        /// This does NOT replace the application's
        /// deterministic triage risk level.
        /// </summary>
        public string Severity { get; set; } = "Info";

        /// <summary>
        /// Patient context used by the AI, when supplied.
        /// </summary>
        public string? PatientContext { get; set; }

        /// <summary>
        /// Mandatory safety disclaimer.
        /// </summary>
        public string Disclaimer { get; set; } =
            "This assistant provides screening support only. " +
            "It does not diagnose disease, prescribe treatment, " +
            "or replace a qualified healthcare professional.";

        /// <summary>
        /// Time at which the AI response was generated.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}