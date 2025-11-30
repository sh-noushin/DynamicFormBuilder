using System.Text.Json;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;

namespace FormBuilder.Core.Services;

public class FieldValidator : IFieldValidator
{
    private readonly FormBuilder.Models.Repositories.IFormVersionRepository _versionRepository;

    public FieldValidator(FormBuilder.Models.Repositories.IFormVersionRepository versionRepository)
    {
        _versionRepository = versionRepository;
    }

    public async Task<Dictionary<string, List<string>>> ValidateAsync(Guid formVersionId, Dictionary<string, string?>? fieldValues)
    {
        var errors = new Dictionary<string, List<string>>();

        var version = await _versionRepository.GetVersionByIdAsync(formVersionId);
        if (version == null)
        {
            errors["__form"] = new List<string> { $"Form version {formVersionId} not found." };
            return errors;
        }

        // Normalize incoming values map
        fieldValues ??= new Dictionary<string, string?>();

        foreach (var field in version.Fields.OrderBy(f => f.Order))
        {
            var fieldErrors = new List<string>();
            fieldValues.TryGetValue(field.Name, out var rawValue);
            var hasValue = !string.IsNullOrEmpty(rawValue);

            // Required check
            if (field.IsRequired && !hasValue)
                fieldErrors.Add("This field is required.");

            // If there's a validation JSON, try to parse it and apply rules
            if (!string.IsNullOrEmpty(field.Validation) && hasValue)
            {
                try
                {
                    using var doc = JsonDocument.Parse(field.Validation);
                    var root = doc.RootElement;

                    // pattern / regex
                    if (root.TryGetProperty("pattern", out var patternEl) && patternEl.ValueKind == JsonValueKind.String)
                    {
                        var pattern = patternEl.GetString()!;
                        try
                        {
                            if (!System.Text.RegularExpressions.Regex.IsMatch(rawValue!, pattern))
                                fieldErrors.Add("Value does not match the required pattern.");
                        }
                        catch
                        {
                            // ignore invalid regex here but surface generic message
                            fieldErrors.Add("Validation pattern is invalid on the field configuration.");
                        }
                    }

                    // minLength / maxLength
                    if (root.TryGetProperty("minLength", out var minLenEl) && minLenEl.ValueKind == JsonValueKind.Number && rawValue != null)
                    {
                        if (rawValue!.Length < minLenEl.GetInt32())
                            fieldErrors.Add($"Minimum length is {minLenEl.GetInt32()}.");
                    }
                    if (root.TryGetProperty("maxLength", out var maxLenEl) && maxLenEl.ValueKind == JsonValueKind.Number && rawValue != null)
                    {
                        if (rawValue!.Length > maxLenEl.GetInt32())
                            fieldErrors.Add($"Maximum length is {maxLenEl.GetInt32()}.");
                    }

                    // numeric min/max
                    if (field.Type == FieldType.Number && double.TryParse(rawValue, out var numeric))
                    {
                        if (root.TryGetProperty("minimum", out var minEl) && minEl.ValueKind == JsonValueKind.Number)
                        {
                            if (numeric < minEl.GetDouble())
                                fieldErrors.Add($"Minimum value is {minEl.GetDouble()}.");
                        }
                        if (root.TryGetProperty("maximum", out var maxEl) && maxEl.ValueKind == JsonValueKind.Number)
                        {
                            if (numeric > maxEl.GetDouble())
                                fieldErrors.Add($"Maximum value is {maxEl.GetDouble()}.");
                        }
                    }

                    // allowed values (enum/select)
                    if (root.TryGetProperty("allowed", out var allowedEl) && allowedEl.ValueKind == JsonValueKind.Array)
                    {
                        var allowed = allowedEl.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        if (!allowed.Contains(rawValue))
                            fieldErrors.Add("Value is not one of the allowed options.");
                    }
                }
                catch (JsonException)
                {
                    fieldErrors.Add("Invalid validation configuration for this field.");
                }
            }

            if (fieldErrors.Any())
                errors[field.Name] = fieldErrors;
        }

        return errors;
    }
}
