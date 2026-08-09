using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;
using ArogyaPulse.Api.DTOs;
namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api/patients")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientRepository _repository;
        private readonly ITriageService _triageService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        public PatientsController(
            IPatientRepository repository,
            ITriageService triageService,
            INotificationService notificationService,
            IMapper mapper)
        {
            _repository = repository;
            _triageService = triageService;
            _notificationService = notificationService;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _repository.GetAllAsync();
            var dto = _mapper.Map<List<PatientResponseDto>>(patients);

            return Ok(new
            {
                success = true,
                data = dto,
                count = dto.Count
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _repository.GetByIdAsync(id);

            if (patient == null)
            {
                return NotFound(new { success = false, message = "Patient not found" });
            }

            var dto = _mapper.Map<PatientResponseDto>(patient);
            return Ok(new { success = true, data = dto });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatientCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Missing required fields" });
            }

            var patient = _mapper.Map<Patient>(createDto);

            patient.RiskScore = _triageService.CalculateScore(
                patient.Bp, patient.SpO2, patient.Temp, patient.Glucose, patient.IsPregnant);
            patient.RiskLevel = _triageService.GetRiskLevel(patient.RiskScore);

            var saved = await _repository.AddAsync(patient);

            if (saved.RiskLevel == "High")
            {
                await _notificationService.SendHighRiskAlertAsync(saved);
            }

            return StatusCode(201, new
            {
                success = true,
                data = new
                {
                    id = saved.Id,
                    name = saved.Name,
                    riskScore = saved.RiskScore,
                    riskLevel = saved.RiskLevel
                }
            });
        }
    }
}