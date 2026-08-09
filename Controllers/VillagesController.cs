using Microsoft.AspNetCore.Mvc;
using ArogyaPulse.Api.Interfaces;

namespace ArogyaPulse.Api.Controllers
{
    [ApiController]
    [Route("api/villages")]
    public class VillagesController : ControllerBase
    {
        private readonly IPatientRepository _repository;

        public VillagesController(IPatientRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetVillageStats()
        {
            var stats = await _repository.GetVillageStatsAsync();
            return Ok(new
            {
                success = true,
                count = stats.Count,
                data = stats
            });
        }
    }
}
