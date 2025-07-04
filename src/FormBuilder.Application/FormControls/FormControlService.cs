using AutoMapper;
using FormBuilder.Application.Contract.FormControls;
using FormBuilder.Application.Contract.FormControls.Dtos.Request;
using FormBuilder.Application.Contract.FormControls.Dtos.Response;
using FormBuilder.Domain.FormControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormControls;

public class FormControlService : IFormControlService
{
    private readonly IFormControlManager _manager;
    private readonly IFormControlRepository _repository;
    private readonly IMapper _mapper;


    public FormControlService(IFormControlManager manager, IFormControlRepository repository, IMapper mapper)
    {
        _manager = manager;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateFormControlRequest request)
    {
        var control = await _manager.CreateAsync(request.Label, request.Type, request.IsRequired, request.FormVersionId);
        return control.Id;
    }

    public async Task UpdateAsync(UpdateFormControlRequest request)
    {
        await _manager.UpdateAsync(request.Id, request.Label, request.Type, request.IsRequired);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<FormControlResponse> GetByIdAsync(Guid id) => _mapper.Map<FormControlResponse>(await _repository.GetAsync(id));
    public async Task<IEnumerable<FormControlResponse>> GetAllAsync() => _mapper.Map<IEnumerable<FormControlResponse>>(await _repository.GetListAsync());
    public async Task<FormControlResponse> GetByLabelAsync(string label) =>
        _mapper.Map<FormControlResponse>((await _repository.GetListAsync(c => c.Label == label)).FirstOrDefault());
}

