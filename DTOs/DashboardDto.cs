namespace ArogyaPulse.Api.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalPatients { get; set; }
        public int HighRisk { get; set; }
        public int MediumRisk { get; set; }
        public int LowRisk { get; set; }
        public double AverageRiskScore { get; set; }
        public double HighRiskPercentage { get; set; }
        public int PendingReview { get; set; }
        public int Referred { get; set; }
        public int Discharged { get; set; }
        public int ConnectedVillages { get; set; }
        public List<VillageStatDto> VillageBreakdown { get; set; } = new();
        public List<DailyTrendDto> DailyTrends { get; set; } = new();
    }

    public class DailyTrendDto
    {
        public string Date { get; set; } = string.Empty;
        public int Screenings { get; set; }
        public int HighRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int LowRiskCount { get; set; }
    }
}
