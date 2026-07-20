using FormBuilder.Core.Interfaces;

namespace FormBuilder.Core.Services.FieldRules;

public sealed class RequiredRule : IFieldRule
{
    public IEnumerable<string> Validate(FieldValidationContext context)
    {
        if (context.Field.IsRequired && !context.HasValue)
            yield return "This field is required.";
    }
}
