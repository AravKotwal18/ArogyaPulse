using Microsoft.AspNetCore.Mvc;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api/sync")]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        /// <summary>
        /// Accept a batch of offline-created patient records from an ASHA worker device.
        /// Performs duplicate detection, triage evaluation, and returns per-record results.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SyncBatch([FromBody] SyncRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid sync request." });
            }

            if (request.Records.Count == 0)
            {
                return BadRequest(new { success = false, message = "No records to sync." });
            }

            var result = await _syncService.ProcessBatchAsync(request);

            return Ok(new
            {
                success = true,
                message = $"Sync complete: {result.Synced} synced, {result.Conflicts} conflicts.",
                data = result
            });
        }

        /// <summary>
        /// Get sync history for a specific device.
        /// </summary>
        [HttpGet("status/{deviceId}")]
        public async Task<IActionResult> GetDeviceStatus(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return BadRequest(new { success = false, message = "Device ID is required." });
            }

            var history = await _syncService.GetDeviceHistoryAsync(deviceId);
            return Ok(new
            {
                success = true,
                deviceId,
                count = history.Count,
                data = history
            });
        }
    }
}
