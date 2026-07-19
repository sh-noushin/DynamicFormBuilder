using System.Text.Json;
using FormBuilder.Models.Entities;

namespace FormBuilder.Core.Services.FieldRules;

public sealed class FieldValidationContext
{
    public FormVersionField Field { get; }
    public string? RawValue { get; }
    public JsonElement? Validation { get; }

    public bool HasValue => !string.IsNullOrEmpty(RawValue);

    public FieldValidationContext(FormVersionField field, string? rawValue, JsonElement? validation)
    {
        Field = field;
        RawValue = rawValue;
        Validation = validation;
    }
}
