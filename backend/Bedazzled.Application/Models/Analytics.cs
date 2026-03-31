namespace Bedazzled.Application.Models;

public class AdminAnalytics
{
    public int TotalBookings { get; set; }
    public Dictionary<string, int> PopularEventTypes { get; set; } = new();
    public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
}

public class MonthlyTrend
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}
