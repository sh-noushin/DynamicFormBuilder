using System.Text.Json;
using FormBuilder.Core.Interfaces;

namespace FormBuilder.Core.Services.FieldRules;

public sealed class AllowedValuesRule : IFieldRule
{
    public IEnumerable<string> Validate(FieldValidationContext context)
    {
        if (!context.HasValue || context.Validation is not { } root)
            yield break;
        if (!root.TryGetProperty("allowed", out var allowedEl) || allowedEl.ValueKind != JsonValueKind.Array)
            yield break;

        var allowed = allowedEl.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!allowed.Contains(context.RawValue))
            yield return "Value is not one of the allowed options.";
    }
}
