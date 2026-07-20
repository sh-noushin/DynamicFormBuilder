using System.Text.Json;
using FormBuilder.Core.Interfaces;

namespace FormBuilder.Core.Services.FieldRules;

public sealed class LengthRule : IFieldRule
{
    public IEnumerable<string> Validate(FieldValidationContext context)
    {
        if (!context.HasValue || context.Validation is not { } root)
            yield break;

        var value = context.RawValue!;
        var customMessage = root.TryGetProperty("lengthMessage", out var msgEl)
                            && msgEl.ValueKind == JsonValueKind.String
                            && !string.IsNullOrWhiteSpace(msgEl.GetString())
            ? msgEl.GetString()
            : null;

        if (root.TryGetProperty("minLength", out var minEl) && minEl.ValueKind == JsonValueKind.Number)
        {
            var min = minEl.GetInt32();
            if (value.Length < min)
                yield return customMessage ?? $"Minimum length is {min}.";
        }

        if (root.TryGetProperty("maxLength", out var maxEl) && maxEl.ValueKind == JsonValueKind.Number)
        {
            var max = maxEl.GetInt32();
            if (value.Length > max)
                yield return customMessage ?? $"Maximum length is {max}.";
        }
    }
}
