using FormBuilder.Application.Contract.FormControls.Dtos.Request;
using FormBuilder.Application.Contract.FormControls.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormControls;

public interface IFormControlService
{
    Task<Guid> CreateAsync(CreateFormControlRequest request);
    Task UpdateAsync(UpdateFormControlRequest request);
    Task DeleteAsync(Guid id);
    Task<FormControlResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<FormControlResponse>> GetAllAsync();
    Task<FormControlResponse> GetByLabelAsync(string label);

}
