using LeaderboardApi.Models;
using LeaderboardApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaderboardApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var entries = await _leaderboardService.GetLeaderboardAsync();
            return Ok(new LeaderboardEntriesWrapper() { entries = entries });
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] LeaderboardEntry entry)
        {
            await _leaderboardService.SubmitScoreAsync(entry);
            return Ok();
        }
    }
}