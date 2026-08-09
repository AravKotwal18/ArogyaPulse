namespace ArogyaPulse.Api.DTOs
{
    public class StatsDto
    {
        public int TotalPatients { get; set; }
        public int HighRisk { get; set; }
        public int MediumRisk { get; set; }
        public int LowRisk { get; set; }
        public string AvgReferralTime { get; set; } = "18 min";
        public string SyncAccuracy { get; set; } = "99.4%";
    }
}