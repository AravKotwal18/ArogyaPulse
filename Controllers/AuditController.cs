using Microsoft.AspNetCore.Mvc;
using ArogyaPulse.Api.Interfaces;

namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api/audit")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Get audit trail for a specific patient.
        /// </summary>
        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var logs = await _auditService.GetByPatientIdAsync(patientId);
            return Ok(new
            {
                success = true,
                patientId,
                count = logs.Count,
                data = logs
            });
        }

        /// <summary>
        /// Get recent audit events (admin view).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 50)
        {
            var clamped = Math.Clamp(count, 1, 200);
            var logs = await _auditService.GetRecentAsync(clamped);
            return Ok(new
            {
                success = true,
                count = logs.Count,
                data = logs
            });
        }
    }
}
