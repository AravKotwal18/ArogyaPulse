namespace ArogyaPulse.Api.DTOs
{
    public class PatientResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = "Unknown";
        public string BloodGroup { get; set; } = "Unknown";
        public string Village { get; set; } = string.Empty;
        public string Bp { get; set; } = string.Empty;
        public int SpO2 { get; set; }
        public double Temp { get; set; }
        public int Glucose { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public bool IsPregnant { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string DoctorNotes { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public VitalsDto Vitals { get; set; } = new();
    }
}