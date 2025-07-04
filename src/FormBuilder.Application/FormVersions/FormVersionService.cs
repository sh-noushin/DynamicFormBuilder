using AutoMapper;
using FormBuilder.Application.Contract.FormVersions;
using FormBuilder.Application.Contract.FormVersions.Dtos.Request;
using FormBuilder.Application.Contract.FormVersions.Dtos.Response;
using FormBuilder.Domain.FormVersions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormVersions;

public class FormVersionService : IFormVersionService
{
    private readonly IFormVersionManager _manager;
    private readonly IFormVersionRepository _repository;
    private readonly IMapper _mapper;

    public FormVersionService(
        IFormVersionManager manager,
        IFormVersionRepository repository,
        IMapper mapper)
    {
        _manager = manager;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateFormVersionRequest request)
    {
        var formVersion = await _manager.CreateAsync(request.Version, request.FormId);
        return formVersion.Id;
    }

    public async Task UpdateVersionAsync(UpdateFormVersionRequest request)
    {
        await _manager.UpdateVersionAsync(request.Id, request.Version);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<FormVersionResponse> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return _mapper.Map<FormVersionResponse>(entity);
    }

    public async Task<IEnumerable<FormVersionResponse>> GetAllAsync()
    {
        var list = await _repository.GetListAsync();
        return _mapper.Map<IEnumerable<FormVersionResponse>>(list);
    }

    public async Task<IEnumerable<FormVersionResponse>> GetListWithControlsAsync()
    {
        var list = await _repository.GetListWithControlsAsync();
        return _mapper.Map<IEnumerable<FormVersionResponse>>(list);
    }

    public async Task<FormVersionResponse> GetByVersionAsync(string version)
    {
        var list = await _repository.GetListAsync(v => v.Version == version);
        return _mapper.Map<FormVersionResponse>(list.FirstOrDefault());
    }
}
