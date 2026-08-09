namespace ArogyaPulse.Api.DTOs
{
    public class TriageResultDto
    {
        public int TotalScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> Breakdown { get; set; } = new();
        public List<string> ActionRecommendations { get; set; } = new();
        public string TriageProtocol { get; set; } = "ArogyaPulse Screening Protocol v1";
        public string Disclaimer { get; set; } = "This is a screening aid, not a clinical diagnosis. All findings must be reviewed by a qualified healthcare professional.";
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    }
}