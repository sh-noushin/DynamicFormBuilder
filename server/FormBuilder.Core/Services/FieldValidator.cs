using System.Text.Json;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Services.FieldRules;

namespace FormBuilder.Core.Services;

public class FieldValidator : IFieldValidator
{
    private readonly FormBuilder.Models.Repositories.IFormVersionRepository _versionRepository;
    private readonly IReadOnlyList<IFieldRule> _rules;

    public FieldValidator(FormBuilder.Models.Repositories.IFormVersionRepository versionRepository, IEnumerable<IFieldRule> rules)
    {
        _versionRepository = versionRepository;
        _rules = rules.ToList();
    }

    // Convenience constructor for tests and standalone usage; wires the built-in rule set.
    public FieldValidator(FormBuilder.Models.Repositories.IFormVersionRepository versionRepository)
        : this(versionRepository, DefaultRules())
    {
    }

    public static IEnumerable<IFieldRule> DefaultRules() => new IFieldRule[]
    {
        new RequiredRule(),
        new PatternRule(),
        new LengthRule(),
        new NumericRangeRule(),
        new AllowedValuesRule()
    };

    public async Task<Dictionary<string, List<string>>> ValidateAsync(Guid formVersionId, Dictionary<string, string?>? fieldValues)
    {
        var errors = new Dictionary<string, List<string>>();

        var version = await _versionRepository.GetVersionByIdAsync(formVersionId);
        if (version == null)
        {
            errors["__form"] = new List<string> { $"Form version {formVersionId} not found." };
            return errors;
        }

        fieldValues ??= new Dictionary<string, string?>();

        foreach (var field in version.Fields.OrderBy(f => f.Order))
        {
            fieldValues.TryGetValue(field.Name, out var rawValue);

            var fieldErrors = new List<string>();

            JsonElement? validationRoot = null;
            JsonDocument? doc = null;

            if (!string.IsNullOrEmpty(field.Validation))
            {
                try
                {
                    doc = JsonDocument.Parse(field.Validation);
                    validationRoot = doc.RootElement.Clone();
                }
                catch (JsonException)
                {
                    fieldErrors.Add("Invalid validation configuration for this field.");
                }
                finally
                {
                    doc?.Dispose();
                }
            }

            var context = new FieldValidationContext(field, rawValue, validationRoot);

            // If the config JSON is invalid, skip config-dependent rules; still run required.
            foreach (var rule in _rules)
            {
                if (validationRoot is null && rule is not RequiredRule)
                    continue;
                fieldErrors.AddRange(rule.Validate(context));
            }

            if (fieldErrors.Any())
                errors[field.Name] = fieldErrors;
        }

        return errors;
    }
}
