using System.ComponentModel.DataAnnotations;
namespace ArogyaPulse.Api.DTOs
{
    public class VitalsDto
    {
        [Required(ErrorMessage = "Blood Pressure is required")]
        [RegularExpression(@"^\d{2,3}\s*/\s*\d{2,3}$", ErrorMessage = "BP must be in format like 120/80")]
        public string Bp { get; set; } = string.Empty;
        [Range(50, 100, ErrorMessage = "SpO2 must be between 50 and 100")]
        public int SpO2 { get; set; }
        [Range(30.0, 45.0, ErrorMessage = "Temperature must be between 30 and 45°C")]
        public double Temp { get; set; }
        [Range(20, 600, ErrorMessage = "Glucose must be between 20 and 600 mg/dL")]
        public int Glucose { get; set; }
    }
}