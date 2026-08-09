using System.ComponentModel.DataAnnotations;
namespace ArogyaPulse.Api.DTOs
{
    public class PatientUpdateDto
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public string? Village { get; set; }
        public VitalsDto? Vitals { get; set; }
        public string? Symptoms { get; set; }
        public bool? IsPregnant { get; set; }
        public string? Status { get; set; }
        public string? DoctorNotes { get; set; }
    }
}