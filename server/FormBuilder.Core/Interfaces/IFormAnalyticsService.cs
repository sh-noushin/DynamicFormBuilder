using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IFormAnalyticsService
{
    // Returns submission analytics for the form. days controls the width of
    // the DailyCounts window (clamped to a sane range). Timestamps are bucketed
    // by UTC calendar date.
    Task<FormAnalyticsDto> GetFormAnalyticsAsync(Guid formId, int days);
}
