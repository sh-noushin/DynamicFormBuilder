using FormBuilder.Core.Services.FieldRules;

namespace FormBuilder.Core.Interfaces;

public interface IFieldRule
{
    IEnumerable<string> Validate(FieldValidationContext context);
}
