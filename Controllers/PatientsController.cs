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
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;

        public PatientsController(
            IPatientRepository repository,
            ITriageService triageService,
            INotificationService notificationService,
            IAuditService auditService,
            IMapper mapper)
        {
            _repository = repository;
            _triageService = triageService;
            _notificationService = notificationService;
            _auditService = auditService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? village,
            [FromQuery] string? riskLevel,
            [FromQuery] string? search)
        {
            var patients = await _repository.GetAllAsync(village, riskLevel, search);
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
            var triageEval = _triageService.EvaluateTriage(
                patient.Bp, patient.SpO2, patient.Temp, patient.Glucose, patient.IsPregnant);
            return Ok(new
            {
                success = true,
                data = dto,
                triageEvaluation = triageEval
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatientCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Missing required fields" });
            }

            var patient = _mapper.Map<Patient>(createDto);
            if (patient.Gender != "Female") patient.IsPregnant = false;

            var triageEval = _triageService.EvaluateTriage(
                patient.Bp, patient.SpO2, patient.Temp, patient.Glucose, patient.IsPregnant);
            patient.RiskScore = triageEval.TotalScore;
            patient.RiskLevel = triageEval.RiskLevel;
            patient.Status = "Pending";

            var saved = await _repository.AddAsync(patient);

            // Audit: patient registration
            await _auditService.LogAsync(
                saved.Id, "PatientCreated", "ASHA",
                $"New patient registered. Risk: {triageEval.RiskLevel} ({triageEval.TotalScore}/100)");

            if (saved.RiskLevel == "High")
            {
                await _notificationService.SendHighRiskAlertAsync(saved);
                await _auditService.LogAsync(
                    saved.Id, "HighRiskAlert", "System",
                    "High-risk alert triggered for immediate doctor review.");
            }

            var responseDto = _mapper.Map<PatientResponseDto>(saved);
            return StatusCode(201, new
            {
                success = true,
                message = "Patient registered and triage evaluated successfully.",
                data = responseDto,
                triageEvaluation = triageEval
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientUpdateDto updateDto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { success = false, message = "Patient not found" });
            }

            // Track changes for audit
            var changes = new List<string>();

            if (!string.IsNullOrWhiteSpace(updateDto.Name) && updateDto.Name != existing.Name)
            {
                changes.Add($"Name: {existing.Name} → {updateDto.Name}");
                existing.Name = updateDto.Name;
            }
            if (updateDto.Age.HasValue && updateDto.Age.Value != existing.Age)
            {
                changes.Add($"Age: {existing.Age} → {updateDto.Age.Value}");
                existing.Age = updateDto.Age.Value;
            }
            if (!string.IsNullOrWhiteSpace(updateDto.Gender) && updateDto.Gender != existing.Gender)
            {
                changes.Add($"Gender: {existing.Gender} → {updateDto.Gender}");
                existing.Gender = updateDto.Gender;
            }
            if (!string.IsNullOrWhiteSpace(updateDto.BloodGroup) && updateDto.BloodGroup != existing.BloodGroup)
            {
                changes.Add($"BloodGroup: {existing.BloodGroup} → {updateDto.BloodGroup}");
                existing.BloodGroup = updateDto.BloodGroup;
            }
            if (!string.IsNullOrWhiteSpace(updateDto.Village) && updateDto.Village != existing.Village)
            {
                changes.Add($"Village: {existing.Village} → {updateDto.Village}");
                existing.Village = updateDto.Village;
            }

            if (existing.Gender != "Female")
                existing.IsPregnant = false;
            else if (updateDto.IsPregnant.HasValue)
                existing.IsPregnant = updateDto.IsPregnant.Value;

            if (updateDto.Symptoms != null) existing.Symptoms = updateDto.Symptoms;

            if (!string.IsNullOrWhiteSpace(updateDto.Status) && updateDto.Status != existing.Status)
            {
                changes.Add($"Status: {existing.Status} → {updateDto.Status}");
                existing.Status = updateDto.Status;
            }
            if (updateDto.DoctorNotes != null && updateDto.DoctorNotes != existing.DoctorNotes)
            {
                changes.Add("DoctorNotes updated");
                existing.DoctorNotes = updateDto.DoctorNotes;
            }

            if (updateDto.Vitals != null)
            {
                if (!string.IsNullOrWhiteSpace(updateDto.Vitals.Bp)) existing.Bp = updateDto.Vitals.Bp;
                if (updateDto.Vitals.SpO2 > 0) existing.SpO2 = updateDto.Vitals.SpO2;
                if (updateDto.Vitals.Temp > 0) existing.Temp = updateDto.Vitals.Temp;
                if (updateDto.Vitals.Glucose > 0) existing.Glucose = updateDto.Vitals.Glucose;
            }

            var triageEval = _triageService.EvaluateTriage(
                existing.Bp, existing.SpO2, existing.Temp, existing.Glucose, existing.IsPregnant);
            string previousRisk = existing.RiskLevel;
            existing.RiskScore = triageEval.TotalScore;
            existing.RiskLevel = triageEval.RiskLevel;

            if (previousRisk != existing.RiskLevel)
            {
                changes.Add($"RiskLevel: {previousRisk} → {existing.RiskLevel}");
            }

            var updated = await _repository.UpdateAsync(existing);

            // Audit: patient update
            if (changes.Count > 0)
            {
                await _auditService.LogAsync(
                    id, "PatientUpdated", "Doctor",
                    string.Join("; ", changes));
            }

            if (updated!.RiskLevel == "High" && previousRisk != "High")
            {
                await _notificationService.SendHighRiskAlertAsync(updated);
                await _auditService.LogAsync(
                    id, "HighRiskEscalation", "System",
                    $"Risk escalated from {previousRisk} to High.");
            }

            var dto = _mapper.Map<PatientResponseDto>(updated);
            return Ok(new
            {
                success = true,
                message = "Patient updated successfully.",
                data = dto,
                triageEvaluation = triageEval
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { success = false, message = "Patient not found" });
            }

            await _auditService.LogAsync(id, "PatientDeleted", "Admin", $"Patient #{id} removed.");

            return Ok(new { success = true, message = $"Patient #{id} removed successfully." });
        }
    }
}