using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;
using System.Globalization;
using System.Text.Json;

namespace FormBuilder.Core.Services;

public class FormAnalyticsService : IFormAnalyticsService
{
    // Sensible bounds so a caller can't blow up the response by asking for
    // "days=100000". 30 is the default the UI ships with.
    private const int MinDays = 1;
    private const int MaxDays = 365;

    private readonly IFormRepository _formRepository;
    private readonly IFormSubmissionRepository _submissionRepository;

    public FormAnalyticsService(
        IFormRepository formRepository,
        IFormSubmissionRepository submissionRepository)
    {
        _formRepository = formRepository;
        _submissionRepository = submissionRepository;
    }

    public async Task<FormAnalyticsDto> GetFormAnalyticsAsync(Guid formId, int days)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));

        var form = await _formRepository.GetByIdAsync(formId)
            ?? throw new FormNotFoundException(formId);

        var window = Math.Clamp(days, MinDays, MaxDays);
        var today = DateTime.UtcNow.Date;
        var windowStart = today.AddDays(-(window - 1));

        var totalCount = await _submissionRepository.GetSubmissionCountByFormIdAsync(form.Id);
        var timestamps = await _submissionRepository
            .GetSubmittedAtByFormIdSinceAsync(form.Id, today.AddDays(-Math.Max(window, 30)));

        var perDay = timestamps
            .GroupBy(ts => ts.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var dailyCounts = new List<DailyCountDto>(window);
        for (var i = 0; i < window; i++)
        {
            var day = windowStart.AddDays(i);
            dailyCounts.Add(new DailyCountDto
            {
                Date = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Count = perDay.TryGetValue(day, out var c) ? c : 0
            });
        }

        var last7Start = today.AddDays(-6);
        var last30Start = today.AddDays(-29);

        var breakdowns = await BuildFieldBreakdownsAsync(form);

        return new FormAnalyticsDto
        {
            TotalSubmissions = totalCount,
            Last7Days = timestamps.Count(ts => ts.Date >= last7Start),
            Last30Days = timestamps.Count(ts => ts.Date >= last30Start),
            DailyCounts = dailyCounts,
            FieldBreakdowns = breakdowns
        };
    }

    // Per-field distribution counters for choice-type fields on the current
    // version. Runs across ALL submissions of the form (not just the daily
    // window) so an admin can see a stable histogram regardless of how narrow
    // the date range they're looking at is.
    private async Task<List<FieldBreakdownDto>> BuildFieldBreakdownsAsync(Form form)
    {
        var currentVersion = form.Versions.FirstOrDefault(v => v.IsCurrentVersion);
        if (currentVersion == null || currentVersion.Fields.Count == 0)
            return new List<FieldBreakdownDto>();

        var choiceFields = currentVersion.Fields
            .OrderBy(f => f.Order)
            .Where(f => f.Type == FieldType.Radio
                     || f.Type == FieldType.Select
                     || f.Type == FieldType.Checkbox
                     || f.Type == FieldType.Rating)
            .ToList();
        if (choiceFields.Count == 0)
            return new List<FieldBreakdownDto>();

        var submissions = await _submissionRepository.GetByFormIdAsync(form.Id);

        // Precompute name -> raw values across all submissions once, so we
        // don't iterate the full submission list per field.
        var valuesByField = new Dictionary<string, List<string>>();
        foreach (var s in submissions)
        {
            foreach (var v in s.Values)
            {
                if (string.IsNullOrEmpty(v.FieldName)) continue;
                if (!valuesByField.TryGetValue(v.FieldName, out var list))
                {
                    list = new List<string>();
                    valuesByField[v.FieldName] = list;
                }
                if (!string.IsNullOrEmpty(v.FieldValue)) list.Add(v.FieldValue!);
            }
        }

        var result = new List<FieldBreakdownDto>(choiceFields.Count);
        foreach (var field in choiceFields)
        {
            valuesByField.TryGetValue(field.Name, out var raw);
            raw ??= new List<string>();
            var buckets = BuildBucketsFor(field, raw);
            result.Add(new FieldBreakdownDto
            {
                FieldName = field.Name,
                FieldLabel = field.Label,
                FieldType = field.Type.ToString(),
                TotalAnswered = raw.Count,
                Buckets = buckets,
            });
        }
        return result;
    }

    private static List<BreakdownBucketDto> BuildBucketsFor(FormVersionField field, List<string> rawValues)
    {
        switch (field.Type)
        {
            case FieldType.Checkbox:
            {
                var yes = rawValues.Count(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase));
                var no = rawValues.Count - yes;
                return new List<BreakdownBucketDto>
                {
                    new() { Label = "Yes", Count = yes },
                    new() { Label = "No",  Count = no  },
                };
            }
            case FieldType.Rating:
            {
                var counts = new int[5];
                foreach (var v in rawValues)
                {
                    if (int.TryParse(v, out var n) && n >= 1 && n <= 5) counts[n - 1]++;
                }
                var list = new List<BreakdownBucketDto>(5);
                for (var i = 0; i < 5; i++)
                {
                    list.Add(new BreakdownBucketDto { Label = $"{i + 1} ★", Count = counts[i] });
                }
                return list;
            }
            case FieldType.Radio:
            case FieldType.Select:
            {
                var options = ParseOptions(field.Options);
                if (options.Count == 0)
                {
                    // No declared options - just group observed values.
                    return rawValues
                        .GroupBy(v => v)
                        .OrderByDescending(g => g.Count())
                        .Select(g => new BreakdownBucketDto { Label = g.Key, Count = g.Count() })
                        .ToList();
                }
                return options
                    .Select(opt => new BreakdownBucketDto
                    {
                        Label = opt.Label,
                        Count = rawValues.Count(v => v == opt.Value),
                    })
                    .ToList();
            }
            default:
                return new List<BreakdownBucketDto>();
        }
    }

    private record OptionEntry(string Value, string Label);

    // Options are stored either as a JSON array of {label, value} pairs or as
    // a legacy CSV. Same parsing rules the public form uses so buckets line
    // up with what the user actually saw.
    private static List<OptionEntry> ParseOptions(string? optionsJsonOrCsv)
    {
        if (string.IsNullOrWhiteSpace(optionsJsonOrCsv)) return new List<OptionEntry>();

        try
        {
            using var doc = JsonDocument.Parse(optionsJsonOrCsv);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<OptionEntry>();
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.String)
                    {
                        var s = el.GetString() ?? string.Empty;
                        list.Add(new OptionEntry(s, s));
                    }
                    else if (el.ValueKind == JsonValueKind.Object)
                    {
                        var value = el.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String
                            ? vEl.GetString() ?? string.Empty
                            : string.Empty;
                        var label = el.TryGetProperty("label", out var lEl) && lEl.ValueKind == JsonValueKind.String
                            ? lEl.GetString() ?? value
                            : value;
                        if (value.Length > 0) list.Add(new OptionEntry(value, label));
                    }
                }
                return list;
            }
        }
        catch (JsonException) { /* fall through to CSV */ }

        return optionsJsonOrCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => new OptionEntry(s, s))
            .ToList();
    }
}
