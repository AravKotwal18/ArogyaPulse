namespace ArogyaPulse.Api.DTOs
{
    public class TriageResultDto
    {
        public int TotalScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> Breakdown { get; set; } = new();
        public List<string> ActionRecommendations { get; set; } = new();
    }
}