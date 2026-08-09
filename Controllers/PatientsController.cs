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

        // ---------------------------------------------------------
        // GET: api/patients
        // ---------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? village,
            [FromQuery] string? riskLevel,
            [FromQuery] string? search)
        {
            var patients = await _repository.GetAllAsync(
                village,
                riskLevel,
                search);

            var dto = _mapper.Map<List<PatientResponseDto>>(patients);

            return Ok(new
            {
                success = true,
                data = dto,
                count = dto.Count
            });
        }

        // ---------------------------------------------------------
        // GET: api/patients/{id}
        // ---------------------------------------------------------

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid patient ID."
                });
            }

            var patient = await _repository.GetByIdAsync(id);

            if (patient == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found."
                });
            }

            var dto = _mapper.Map<PatientResponseDto>(patient);

            var triageEvaluation = _triageService.EvaluateTriage(
                patient.Bp,
                patient.SpO2,
                patient.Temp,
                patient.Glucose,
                patient.IsPregnant);

            return Ok(new
            {
                success = true,
                data = dto,
                triageEvaluation
            });
        }

        // ---------------------------------------------------------
        // POST: api/patients
        // ---------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] PatientCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (createDto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Patient data is required."
                });
            }

            // -----------------------------------------------------
            // Map DTO -> Entity
            // -----------------------------------------------------

            var patient = _mapper.Map<Patient>(createDto);

            // -----------------------------------------------------
            // Safety rule:
            // Pregnancy should only be true for Female.
            // -----------------------------------------------------

            if (!string.Equals(
                    patient.Gender,
                    "Female",
                    StringComparison.OrdinalIgnoreCase))
            {
                patient.IsPregnant = false;
            }

            // -----------------------------------------------------
            // Calculate triage
            // -----------------------------------------------------

            var triageEvaluation = _triageService.EvaluateTriage(
                patient.Bp,
                patient.SpO2,
                patient.Temp,
                patient.Glucose,
                patient.IsPregnant);

            patient.RiskScore = triageEvaluation.TotalScore;
            patient.RiskLevel = triageEvaluation.RiskLevel;
            patient.Status = "Pending";
            patient.Timestamp = DateTime.UtcNow;

            // -----------------------------------------------------
            // Save patient
            // -----------------------------------------------------

            var saved = await _repository.AddAsync(patient);

            // -----------------------------------------------------
            // Audit: patient created
            // -----------------------------------------------------

            await _auditService.LogAsync(
                saved.Id,
                "PatientCreated",
                "ASHA",
                $"Patient registered. Risk={saved.RiskLevel}, Score={saved.RiskScore}");

            // -----------------------------------------------------
            // Audit: triage evaluated
            // -----------------------------------------------------

            await _auditService.LogAsync(
                saved.Id,
                "TriageEvaluated",
                "System",
                $"Initial triage completed. Risk={saved.RiskLevel}, Score={saved.RiskScore}");

            // -----------------------------------------------------
            // High-risk alert
            // -----------------------------------------------------

            if (string.Equals(
                    saved.RiskLevel,
                    "High",
                    StringComparison.OrdinalIgnoreCase))
            {
                await _notificationService.SendHighRiskAlertAsync(saved);

                await _auditService.LogAsync(
                    saved.Id,
                    "HighRiskAlert",
                    "System",
                    "High-risk patient added to doctor review queue.");
            }

            var responseDto =
                _mapper.Map<PatientResponseDto>(saved);

            return CreatedAtAction(
                nameof(GetById),
                new { id = saved.Id },
                new
                {
                    success = true,
                    message = "Patient registered and triage evaluated successfully.",
                    data = responseDto,
                    triageEvaluation
                });
        }

        // ---------------------------------------------------------
        // PUT: api/patients/{id}
        // ---------------------------------------------------------

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] PatientUpdateDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid patient ID."
                });
            }

            if (updateDto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Update data is required."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found."
                });
            }

            var changes = new List<string>();

            // -----------------------------------------------------
            // Basic patient information
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(updateDto.Name) &&
                updateDto.Name != existing.Name)
            {
                changes.Add("Name updated");
                existing.Name = updateDto.Name.Trim();
            }

            if (updateDto.Age.HasValue &&
                updateDto.Age.Value != existing.Age)
            {
                changes.Add(
                    $"Age changed from {existing.Age} to {updateDto.Age.Value}");

                existing.Age = updateDto.Age.Value;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Gender) &&
                !string.Equals(
                    updateDto.Gender,
                    existing.Gender,
                    StringComparison.OrdinalIgnoreCase))
            {
                changes.Add("Gender updated");
                existing.Gender = updateDto.Gender.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateDto.BloodGroup) &&
                !string.Equals(
                    updateDto.BloodGroup,
                    existing.BloodGroup,
                    StringComparison.OrdinalIgnoreCase))
            {
                changes.Add("Blood group updated");
                existing.BloodGroup = updateDto.BloodGroup.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Village) &&
                !string.Equals(
                    updateDto.Village,
                    existing.Village,
                    StringComparison.OrdinalIgnoreCase))
            {
                changes.Add("Village updated");
                existing.Village = updateDto.Village.Trim();
            }

            // -----------------------------------------------------
            // Pregnancy
            // -----------------------------------------------------

            bool previousPregnancyStatus = existing.IsPregnant;

            if (string.Equals(
                    existing.Gender,
                    "Female",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (updateDto.IsPregnant.HasValue)
                {
                    existing.IsPregnant =
                        updateDto.IsPregnant.Value;
                }
            }
            else
            {
                existing.IsPregnant = false;
            }

            if (previousPregnancyStatus != existing.IsPregnant)
            {
                changes.Add(
                    $"Pregnancy status changed from {previousPregnancyStatus} to {existing.IsPregnant}");
            }

            // -----------------------------------------------------
            // Symptoms
            // -----------------------------------------------------

            if (updateDto.Symptoms != null &&
                updateDto.Symptoms != existing.Symptoms)
            {
                changes.Add("Symptoms updated");
                existing.Symptoms = updateDto.Symptoms.Trim();
            }

            // -----------------------------------------------------
            // Status
            // -----------------------------------------------------

            string previousStatus = existing.Status;

            if (!string.IsNullOrWhiteSpace(updateDto.Status) &&
                !string.Equals(
                    updateDto.Status,
                    existing.Status,
                    StringComparison.OrdinalIgnoreCase))
            {
                existing.Status = updateDto.Status.Trim();

                changes.Add(
                    $"Status changed from '{previousStatus}' to '{existing.Status}'");
            }

            // -----------------------------------------------------
            // Doctor notes
            // -----------------------------------------------------

            bool doctorNotesChanged =
                updateDto.DoctorNotes != null &&
                updateDto.DoctorNotes != existing.DoctorNotes;

            if (doctorNotesChanged)
            {
                existing.DoctorNotes =
                    updateDto.DoctorNotes!.Trim();

                changes.Add("Doctor notes updated");
            }

            // -----------------------------------------------------
            // Vitals
            // -----------------------------------------------------

            if (updateDto.Vitals != null)
            {
                var vitals = updateDto.Vitals;

                if (!string.IsNullOrWhiteSpace(vitals.Bp) &&
                    !string.Equals(
                        vitals.Bp,
                        existing.Bp,
                        StringComparison.OrdinalIgnoreCase))
                {
                    changes.Add(
                        $"Blood pressure changed from {existing.Bp} to {vitals.Bp}");

                    existing.Bp = vitals.Bp.Trim();
                }

                if (vitals.SpO2 != existing.SpO2)
                {
                    changes.Add(
                        $"SpO2 changed from {existing.SpO2} to {vitals.SpO2}");

                    existing.SpO2 = vitals.SpO2;
                }

                if (Math.Abs(vitals.Temp - existing.Temp) > 0.001)
                {
                    changes.Add(
                        $"Temperature changed from {existing.Temp:F1} to {vitals.Temp:F1}");

                    existing.Temp = vitals.Temp;
                }

                if (vitals.Glucose != existing.Glucose)
                {
                    changes.Add(
                        $"Glucose changed from {existing.Glucose} to {vitals.Glucose}");

                    existing.Glucose = vitals.Glucose;
                }
            }

            // -----------------------------------------------------
            // Recalculate triage after ANY relevant change
            // -----------------------------------------------------

            string previousRiskLevel = existing.RiskLevel;
            int previousRiskScore = existing.RiskScore;

            var triageEvaluation = _triageService.EvaluateTriage(
                existing.Bp,
                existing.SpO2,
                existing.Temp,
                existing.Glucose,
                existing.IsPregnant);

            existing.RiskScore =
                triageEvaluation.TotalScore;

            existing.RiskLevel =
                triageEvaluation.RiskLevel;

            if (previousRiskLevel != existing.RiskLevel)
            {
                changes.Add(
                    $"Risk level changed from '{previousRiskLevel}' to '{existing.RiskLevel}'");
            }

            if (previousRiskScore != existing.RiskScore)
            {
                changes.Add(
                    $"Risk score changed from {previousRiskScore} to {existing.RiskScore}");
            }

            // -----------------------------------------------------
            // Save
            // -----------------------------------------------------

            var updated =
                await _repository.UpdateAsync(existing);

            if (updated == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient could not be updated."
                });
            }

            // -----------------------------------------------------
            // Audit status change
            // -----------------------------------------------------

            if (!string.Equals(
                    previousStatus,
                    updated.Status,
                    StringComparison.OrdinalIgnoreCase))
            {
                await _auditService.LogAsync(
                    id,
                    "StatusChanged",
                    "Doctor",
                    $"Status changed from '{previousStatus}' to '{updated.Status}'.");
            }

            // -----------------------------------------------------
            // Audit doctor note change
            // -----------------------------------------------------

            if (doctorNotesChanged)
            {
                // Do NOT put the actual medical note in the audit log.
                await _auditService.LogAsync(
                    id,
                    "DoctorNoteUpdated",
                    "Doctor",
                    "Doctor notes were updated.");
            }

            // -----------------------------------------------------
            // Referral audit
            // -----------------------------------------------------

            if (!string.Equals(
                    previousStatus,
                    updated.Status,
                    StringComparison.OrdinalIgnoreCase) &&
                updated.Status.Contains(
                    "referred",
                    StringComparison.OrdinalIgnoreCase))
            {
                await _auditService.LogAsync(
                    id,
                    "ReferralCreated",
                    "Doctor",
                    $"Patient status changed to '{updated.Status}'.");
            }

            // -----------------------------------------------------
            // General patient update audit
            // -----------------------------------------------------

            if (changes.Count > 0)
            {
                await _auditService.LogAsync(
                    id,
                    "PatientUpdated",
                    "Doctor",
                    string.Join("; ", changes));
            }

            // -----------------------------------------------------
            // Triage audit
            // -----------------------------------------------------

            await _auditService.LogAsync(
                id,
                "TriageEvaluated",
                "System",
                $"Triage recalculated. Risk={updated.RiskLevel}, Score={updated.RiskScore}");

            // -----------------------------------------------------
            // High-risk escalation
            // -----------------------------------------------------

            bool becameHighRisk =
                string.Equals(
                    updated.RiskLevel,
                    "High",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    previousRiskLevel,
                    "High",
                    StringComparison.OrdinalIgnoreCase);

            if (becameHighRisk)
            {
                await _notificationService
                    .SendHighRiskAlertAsync(updated);

                await _auditService.LogAsync(
                    id,
                    "HighRiskEscalation",
                    "System",
                    $"Risk escalated from '{previousRiskLevel}' to High.");
            }

            // -----------------------------------------------------
            // Response
            // -----------------------------------------------------

            var dto =
                _mapper.Map<PatientResponseDto>(updated);

            return Ok(new
            {
                success = true,
                message = "Patient updated successfully.",
                data = dto,
                triageEvaluation
            });
        }

        // ---------------------------------------------------------
        // DELETE: api/patients/{id}
        // ---------------------------------------------------------

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid patient ID."
                });
            }

            // Check first so we don't report a misleading deletion.
            var patient =
                await _repository.GetByIdAsync(id);

            if (patient == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found."
                });
            }

            var deleted =
                await _repository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient could not be deleted."
                });
            }

            await _auditService.LogAsync(
                id,
                "PatientDeleted",
                "Admin",
                $"Patient #{id} removed.");

            return Ok(new
            {
                success = true,
                message = $"Patient #{id} removed successfully."
            });
        }
    }
}