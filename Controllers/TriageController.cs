using AutoMapper;
using ArogyaPulse.Api.DTOs;
using ArogyaPulse.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class TriageController : ControllerBase
    {
        private readonly IPatientRepository _repository;
        private readonly IMapper _mapper;

        public TriageController(
            IPatientRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("triage-queue")]
        public async Task<IActionResult> GetQueue(
            [FromQuery] string? village,
            [FromQuery] string? status)
        {
            var patients =
                await _repository.GetAllAsync(
                    village,
                    null,
                    null);

            if (!string.IsNullOrWhiteSpace(status))
            {
                patients = patients
                    .Where(x =>
                        string.Equals(
                            x.Status,
                            status,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var riskOrder =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["High"] = 3,
                    ["Medium"] = 2,
                    ["Low"] = 1
                };

            var data =
                _mapper
                    .Map<List<PatientResponseDto>>(patients)
                    .OrderByDescending(x =>
                        riskOrder.GetValueOrDefault(
                            x.RiskLevel,
                            0))
                    .ThenByDescending(x => x.RiskScore)
                    .ThenByDescending(x => x.Timestamp)
                    .ToList();

            return Ok(new
            {
                success = true,

                data,

                stats = new
                {
                    total = data.Count,

                    high = data.Count(
                        x => x.RiskLevel == "High"),

                    medium = data.Count(
                        x => x.RiskLevel == "Medium"),

                    low = data.Count(
                        x => x.RiskLevel == "Low"),

                    pending = data.Count(
                        x => x.Status == "Pending"),

                    referred = data.Count(
                        x => x.Status == "Referred to CHC")
                }
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var patients =
                await _repository.GetAllAsync();

            var villages =
                await _repository.GetVillageStatsAsync();

            var total = patients.Count;

            var high =
                patients.Count(x => x.RiskLevel == "High");

            var medium =
                patients.Count(x => x.RiskLevel == "Medium");

            var low =
                patients.Count(x => x.RiskLevel == "Low");

            var average =
                total == 0
                    ? 0
                    : Math.Round(
                        patients.Average(x => x.RiskScore),
                        1);

            return Ok(new
            {
                success = true,

                data = new
                {
                    totalPatients = total,

                    highRisk = high,

                    mediumRisk = medium,

                    lowRisk = low,

                    averageRiskScore = average,

                    highRiskPercentage =
                        total == 0
                            ? 0
                            : Math.Round(
                                high * 100.0 / total,
                                1),

                    mediumRiskPercentage =
                        total == 0
                            ? 0
                            : Math.Round(
                                medium * 100.0 / total,
                                1),

                    lowRiskPercentage =
                        total == 0
                            ? 0
                            : Math.Round(
                                low * 100.0 / total,
                                1),

                    pendingReview =
                        patients.Count(
                            x => x.Status == "Pending"),

                    referred =
                        patients.Count(
                            x => x.Status == "Referred to CHC"),

                    discharged =
                        patients.Count(
                            x => x.Status == "Discharged"),

                    connectedVillages =
                        villages.Count,

                    villageBreakdown = villages
                }
            });
        }
    }
}