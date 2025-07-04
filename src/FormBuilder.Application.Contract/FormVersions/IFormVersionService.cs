using FormBuilder.Application.Contract.FormVersions.Dtos.Request;
using FormBuilder.Application.Contract.FormVersions.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormVersions;

public interface IFormVersionService
{
    Task<Guid> CreateAsync(CreateFormVersionRequest request);
    Task UpdateVersionAsync(UpdateFormVersionRequest request);
    Task DeleteAsync(Guid id);
    Task<FormVersionResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<FormVersionResponse>> GetAllAsync();
    Task<IEnumerable<FormVersionResponse>> GetListWithControlsAsync();
    Task<FormVersionResponse> GetByVersionAsync(string version);
}