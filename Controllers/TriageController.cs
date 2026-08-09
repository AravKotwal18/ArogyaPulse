using Microsoft.AspNetCore.Mvc;
using ArogyaPulse.Api.Interfaces;
namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class TriageController : ControllerBase
    {
        private readonly IPatientRepository _repository;
        public TriageController(IPatientRepository repository)
        {
            _repository = repository;
        }
        [HttpGet("triage-queue")]
        public async Task<IActionResult> GetQueue()
        {
            var patients = await _repository.GetAllAsync();

            var riskOrder = new Dictionary<string, int> { { "High", 3 }, { "Medium", 2 }, { "Low", 1 } };

            var sorted = patients
                .OrderByDescending(p => riskOrder.GetValueOrDefault(p.RiskLevel, 0))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Age,
                    p.Village,
                    p.Bp,
                    p.SpO2,
                    p.Temp,
                    p.Glucose,
                    p.Symptoms,
                    p.IsPregnant,
                    p.RiskScore,
                    p.RiskLevel,
                    p.Timestamp,
                    vitals = new { p.Bp, p.SpO2, p.Temp, p.Glucose }
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = sorted,
                stats = new
                {
                    total = patients.Count,
                    high = patients.Count(p => p.RiskLevel == "High"),
                    medium = patients.Count(p => p.RiskLevel == "Medium"),
                    low = patients.Count(p => p.RiskLevel == "Low")
                }
            });
        }
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var patients = await _repository.GetAllAsync();

            var stats = new
            {
                totalPatients = patients.Count,
                highRisk = patients.Count(p => p.RiskLevel == "High"),
                mediumRisk = patients.Count(p => p.RiskLevel == "Medium"),
                lowRisk = patients.Count(p => p.RiskLevel == "Low"),
                avgReferralTime = "18 min",
                syncAccuracy = "99.4%"
            };

            return Ok(new { success = true, data = stats });
        }
    }
}