namespace ArogyaPulse.Api.DTOs
{
    public class VitalsDto
    {
        public string Bp { get; set; } = string.Empty;
        public int SpO2 { get; set; }
        public double Temp { get; set; }
        public int Glucose { get; set; }
    }
}