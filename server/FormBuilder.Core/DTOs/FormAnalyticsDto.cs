namespace FormBuilder.Core.DTOs;

public class FormAnalyticsDto
{
    public int TotalSubmissions { get; set; }
    public int Last7Days { get; set; }
    public int Last30Days { get; set; }
    // One entry per calendar day in the requested range, including days with
    // zero submissions. Ordered oldest-first for chart rendering.
    public List<DailyCountDto> DailyCounts { get; set; } = new List<DailyCountDto>();
    // Per-field breakdowns for choice-type fields (Radio, Select, Checkbox,
    // Rating) on the form's current version. Empty for forms with no
    // choice-type fields. Covers ALL submissions (not just the daily window).
    public List<FieldBreakdownDto> FieldBreakdowns { get; set; } = new List<FieldBreakdownDto>();
}

public class FieldBreakdownDto
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public int TotalAnswered { get; set; }
    // Ordered as the field author intended (options in declared order,
    // ratings from 1 to 5). Values without any submissions still appear so
    // the chart shows zero-count buckets.
    public List<BreakdownBucketDto> Buckets { get; set; } = new List<BreakdownBucketDto>();
}

public class BreakdownBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailyCountDto
{
    // ISO date in the analytics timezone (UTC on the server side).
    // Format: "yyyy-MM-dd".
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}
