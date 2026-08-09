using System;
using System.ComponentModel.DataAnnotations;
namespace ArogyaPulse.Api.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int Age { get; set; }
        public string Gender { get; set; } = "Female";
        public string BloodGroup { get; set; } = "Unknown";
        [Required]
        public string Village { get; set; } = string.Empty;
        [Required]
        public string Bp { get; set; } = string.Empty;
        [Required]
        public int SpO2 { get; set; }
        [Required]
        public double Temp { get; set; }
        [Required]
        public int Glucose { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public bool IsPregnant { get; set; } = false;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string DoctorNotes { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}