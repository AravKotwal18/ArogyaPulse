namespace ArogyaPulse.Api.DTOs
{
    public class AiExtractionDto
    {
        public string Language { get; set; } = "unknown";

        public string Intent { get; set; } = "unknown";

        public string? Bp { get; set; }

        public int? SpO2 { get; set; }

        public double? Temp { get; set; }

        public int? Glucose { get; set; }

        public List<string> Symptoms { get; set; } = new();

        public List<string> MissingInformation { get; set; } = new();

        public string AssistantResponse { get; set; } = string.Empty;
    }
}