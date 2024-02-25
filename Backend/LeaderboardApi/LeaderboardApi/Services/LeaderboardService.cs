using System.Text.Json;
using LeaderboardApi.Models;

namespace LeaderboardApi.Services
{
    public class LeaderboardService
    {
        private const string FilePath = "leaderboard.json";

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            if (!File.Exists(FilePath))
            {
                return new List<LeaderboardEntry>();
            }

            using var stream = File.OpenRead(FilePath);
            try
            {
                var leaderboard = await JsonSerializer.DeserializeAsync<List<LeaderboardEntry>>(stream) ?? new List<LeaderboardEntry>();
                return leaderboard ?? new List<LeaderboardEntry>();
            }
            catch (JsonException)
            {
                return new List<LeaderboardEntry>();
            }
        }

        public async Task SubmitScoreAsync(LeaderboardEntry entry)
        {
            var leaderboard = await GetLeaderboardAsync();
            
            var existingEntry = leaderboard.FirstOrDefault(e => e.Name == entry.Name);
            if (existingEntry != null)
            {
                existingEntry.Score = entry.Score;
            }
            else
            {
                leaderboard.Add(entry);
            }

            leaderboard.Sort((x, y) => y.Score.CompareTo(x.Score));

            using var stream = File.Create(FilePath);
            await JsonSerializer.SerializeAsync(stream, leaderboard);
        }
    }
}