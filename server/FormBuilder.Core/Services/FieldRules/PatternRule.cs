using System.Text.Json;
using System.Text.RegularExpressions;
using FormBuilder.Core.Interfaces;

namespace FormBuilder.Core.Services.FieldRules;

public sealed class PatternRule : IFieldRule
{
    public IEnumerable<string> Validate(FieldValidationContext context)
    {
        if (!context.HasValue || context.Validation is not { } root)
            return Array.Empty<string>();
        if (!root.TryGetProperty("pattern", out var patternEl) || patternEl.ValueKind != JsonValueKind.String)
            return Array.Empty<string>();

        var pattern = patternEl.GetString()!;

        try
        {
            return Regex.IsMatch(context.RawValue!, pattern)
                ? Array.Empty<string>()
                : new[] { "Value does not match the required pattern." };
        }
        catch
        {
            return new[] { "Validation pattern is invalid on the field configuration." };
        }
    }
}
