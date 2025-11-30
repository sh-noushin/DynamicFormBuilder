using System.Collections.Generic;

namespace FormBuilder.Core.Interfaces;

public interface IFieldValidator
{
   
    Task<Dictionary<string, List<string>>> ValidateAsync(Guid formVersionId, Dictionary<string, string?>? fieldValues);
}
