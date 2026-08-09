using ArogyaPulse.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api/audit")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(
            IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("{patientId:int}")]
        public async Task<IActionResult> GetPatientAudit(
            int patientId)
        {
            var logs =
                await _auditService
                    .GetByPatientIdAsync(patientId);

            return Ok(new
            {
                success = true,
                patientId,
                count = logs.Count,
                data = logs
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentAudit(
            [FromQuery] int count = 50)
        {
            count = Math.Clamp(count, 1, 200);

            var logs =
                await _auditService
                    .GetRecentAsync(count);

            return Ok(new
            {
                success = true,
                count = logs.Count,
                data = logs
            });
        }
    }
}