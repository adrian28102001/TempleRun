namespace LeaderboardApi.Models;

public class LeaderboardEntriesWrapper
{
    public IList<LeaderboardEntry> entries { get; set; }
}

public class LeaderboardEntry
{
    public string Name { get; set; }
    public int Score { get; set; }
}