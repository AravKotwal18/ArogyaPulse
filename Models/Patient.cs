using System.ComponentModel.DataAnnotations;
namespace ArogyaPulse.Api.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, Range(0, 120)]
        public int Age { get; set; }
        [Required, MaxLength(20)]
        public string Gender { get; set; } = string.Empty;
        [MaxLength(10)]
        public string BloodGroup { get; set; } = "Unknown";
        [Required, MaxLength(100)]
        public string Village { get; set; } = string.Empty;
        [Required, MaxLength(20)]
        public string Bp { get; set; } = string.Empty;
        [Required, Range(0, 100)]
        public int SpO2 { get; set; }
        [Required, Range(30.0, 45.0)]
        public double Temp { get; set; }
        [Required, Range(0, 600)]
        public int Glucose { get; set; }
        [MaxLength(100)]
        public string LocalRecordId { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Symptoms { get; set; } = string.Empty;
        public bool IsPregnant { get; set; } = false;
        public int RiskScore { get; set; }
        [MaxLength(20)]
        public string RiskLevel { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        [MaxLength(1000)]
        public string DoctorNotes { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}