using System.ComponentModel.DataAnnotations;
namespace ArogyaPulse.Api.DTOs
{
    public class PatientCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int Age { get; set; }
        public string Gender { get; set; } = "Female";
        public string BloodGroup { get; set; } = "Unknown";
        [Required]
        public string Village { get; set; } = string.Empty;
        [Required]
        public VitalsDto Vitals { get; set; } = new();
        public string Symptoms { get; set; } = string.Empty;
        public bool IsPregnant { get; set; } = false;
    }
}