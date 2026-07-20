using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;
using System.Globalization;

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

        return new FormAnalyticsDto
        {
            TotalSubmissions = totalCount,
            Last7Days = timestamps.Count(ts => ts.Date >= last7Start),
            Last30Days = timestamps.Count(ts => ts.Date >= last30Start),
            DailyCounts = dailyCounts
        };
    }
}
