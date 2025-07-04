using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues;

public interface IFormSubmissionValueManager
{
    Task<FormSubmissionValue> CreateAsync(Guid formSubmissionId, Guid formControlId, string value);
    Task UpdateValueAsync(Guid id, string newValue);
    Task DeleteAsync(Guid id);
}