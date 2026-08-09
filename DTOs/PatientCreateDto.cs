using System.ComponentModel.DataAnnotations;
namespace ArogyaPulse.Api.DTOs
{
    public class PatientCreateDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        public int Age { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        [MaxLength(20)]
        public string Gender { get; set; } = "Unknown";
        [MaxLength(10)]
        public string BloodGroup { get; set; } = "Unknown";
        [Required(ErrorMessage = "Village is required")]
        [MaxLength(100)]
        public string Village { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vitals are required")]
        public VitalsDto Vitals { get; set; } = new();
        [MaxLength(500)]
        public string Symptoms { get; set; } = string.Empty;
        public bool IsPregnant { get; set; } = false;
    }
}