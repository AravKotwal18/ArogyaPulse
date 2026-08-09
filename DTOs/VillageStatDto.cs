namespace ArogyaPulse.Api.DTOs
{
    public class VillageStatDto
    {
        public string Village { get; set; } = string.Empty;
        public int TotalScreened { get; set; }
        public int HighRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int LowRiskCount { get; set; }
        public double HighRiskPercentage => TotalScreened > 0 ? Math.Round((double)HighRiskCount / TotalScreened * 100, 1) : 0;
        public DateTime LastScreening { get; set; }
        public string PrimaryHealthCenter { get; set; } = string.Empty;
    }
}
