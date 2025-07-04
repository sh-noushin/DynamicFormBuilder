using FormBuilder.Application.Contract.Forms.Dtos.Request;
using FormBuilder.Application.Contract.Forms.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.Forms;

public interface IFormService
{
    Task<Guid> CreateAsync(CreateFormRequest request);
    Task UpdateAsync(UpdateFormRequest request);
    Task DeleteAsync(Guid id);
    Task<Guid> AddVersionAsync(Guid formId, string version);
    Task<FormResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<FormResponse>> GetAllAsync();
    Task<FormResponse> GetByNameAsync(string name);

}
