using FormBuilder.Application.Contract.FormControlValues.Dtos.Request;
using FormBuilder.Application.Contract.FormControlValues.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormControlValues;

public interface IFormControlValueService
{
    Task<Guid> CreateAsync(CreateFormControlValueRequest request);
    Task UpdateAsync(UpdateFormControlValueRequest request);
    Task DeleteAsync(Guid id);
    Task<FormControlValueResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<FormControlValueResponse>> GetAllAsync();
    Task<FormControlValueResponse> GetByValueAsync(string value);

}
