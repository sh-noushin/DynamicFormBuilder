using System.Text.Json;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;

namespace FormBuilder.Core.Services.FieldRules;

public sealed class NumericRangeRule : IFieldRule
{
    public IEnumerable<string> Validate(FieldValidationContext context)
    {
        if (!context.HasValue || context.Field.Type != FieldType.Number || context.Validation is not { } root)
            yield break;

        if (!double.TryParse(context.RawValue, out var numeric))
            yield break;

        var customMessage = root.TryGetProperty("rangeMessage", out var msgEl)
                            && msgEl.ValueKind == JsonValueKind.String
                            && !string.IsNullOrWhiteSpace(msgEl.GetString())
            ? msgEl.GetString()
            : null;

        if (root.TryGetProperty("minimum", out var minEl) && minEl.ValueKind == JsonValueKind.Number)
        {
            var min = minEl.GetDouble();
            if (numeric < min)
                yield return customMessage ?? $"Minimum value is {min}.";
        }

        if (root.TryGetProperty("maximum", out var maxEl) && maxEl.ValueKind == JsonValueKind.Number)
        {
            var max = maxEl.GetDouble();
            if (numeric > max)
                yield return customMessage ?? $"Maximum value is {max}.";
        }
    }
}
