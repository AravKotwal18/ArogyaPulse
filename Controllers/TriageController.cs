using Microsoft.AspNetCore.Mvc;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.DTOs;
using AutoMapper;

namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class TriageController : ControllerBase
    {
        private readonly IPatientRepository _repository;
        private readonly IMapper _mapper;

        public TriageController(IPatientRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("triage-queue")]
        public async Task<IActionResult> GetQueue([FromQuery] string? village, [FromQuery] string? status)
        {
            var patients = await _repository.GetAllAsync(village, null, null);

            if (!string.IsNullOrWhiteSpace(status))
            {
                patients = patients.Where(p => p.Status.ToLower() == status.ToLower()).ToList();
            }

            var riskOrder = new Dictionary<string, int> { { "High", 3 }, { "Medium", 2 }, { "Low", 1 } };

            // Map through AutoMapper — no inline name-based fabrication
            var dtos = _mapper.Map<List<PatientResponseDto>>(patients);

            var sorted = dtos
                .OrderByDescending(p => riskOrder.GetValueOrDefault(p.RiskLevel, 0))
                .ThenByDescending(p => p.RiskScore)
                .ThenByDescending(p => p.Timestamp)
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
                    low = patients.Count(p => p.RiskLevel == "Low"),
                    pending = patients.Count(p => p.Status == "Pending"),
                    referred = patients.Count(p => p.Status == "Referred to CHC")
                }
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var patients = await _repository.GetAllAsync();
            var villageStats = await _repository.GetVillageStatsAsync();

            var stats = new
            {
                totalPatients = patients.Count,
                highRisk = patients.Count(p => p.RiskLevel == "High"),
                mediumRisk = patients.Count(p => p.RiskLevel == "Medium"),
                lowRisk = patients.Count(p => p.RiskLevel == "Low"),
                connectedVillages = villageStats.Count,
                averageRiskScore = patients.Count > 0
                    ? Math.Round(patients.Average(p => p.RiskScore), 1)
                    : 0,
                highRiskPercentage = patients.Count > 0
                    ? Math.Round((double)patients.Count(p => p.RiskLevel == "High") / patients.Count * 100, 1)
                    : 0,
                pendingReview = patients.Count(p => p.Status == "Pending"),
                referred = patients.Count(p => p.Status == "Referred to CHC"),
                discharged = patients.Count(p => p.Status == "Discharged"),
                villageBreakdown = villageStats
            };

            return Ok(new { success = true, data = stats });
        }
    }
}