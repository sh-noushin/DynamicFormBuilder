namespace FormBuilder.Core.DTOs;

public class FormAnalyticsDto
{
    public int TotalSubmissions { get; set; }
    public int Last7Days { get; set; }
    public int Last30Days { get; set; }
    // One entry per calendar day in the requested range, including days with
    // zero submissions. Ordered oldest-first for chart rendering.
    public List<DailyCountDto> DailyCounts { get; set; } = new List<DailyCountDto>();
}

public class DailyCountDto
{
    // ISO date in the analytics timezone (UTC on the server side).
    // Format: "yyyy-MM-dd".
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}
