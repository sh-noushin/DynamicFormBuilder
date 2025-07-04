using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues;

public interface IFormControlValueManager
{
    Task<FormControlValue> CreateAsync(string value, Guid formControlId);
    Task UpdateAsync(Guid id, string value);
    Task DeleteAsync(Guid id);
}
