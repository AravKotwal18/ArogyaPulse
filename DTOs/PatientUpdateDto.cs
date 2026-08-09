using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.DTOs
{
    public class PatientUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [Range(1, 120)]
        public int? Age { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(10)]
        public string? BloodGroup { get; set; }

        [MaxLength(100)]
        public string? Village { get; set; }

        public VitalsDto? Vitals { get; set; }

        [MaxLength(500)]
        public string? Symptoms { get; set; }

        public bool? IsPregnant { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(1000)]
        public string? DoctorNotes { get; set; }
    }
}