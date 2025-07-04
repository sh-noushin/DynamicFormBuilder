using AutoMapper;
using FormBuilder.Application.Contract.FormControlValues;
using FormBuilder.Application.Contract.FormControlValues.Dtos.Request;
using FormBuilder.Application.Contract.FormControlValues.Dtos.Response;
using FormBuilder.Domain.FormControlValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormControlValues;

public class FormControlValueService : IFormControlValueService
{
    private readonly IFormControlValueManager _manager;
    private readonly IFormControlValueRepository _repository;
    private readonly IMapper _mapper;
    public FormControlValueService(IFormControlValueManager manager, IFormControlValueRepository repository, IMapper mapper)
    {
        _manager = manager;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateFormControlValueRequest request)
    {
        var option = await _manager.CreateAsync(request.Value, request.FormControlId);
        return option.Id;
    }

    public async Task UpdateAsync(UpdateFormControlValueRequest request)
    {
        await _manager.UpdateAsync(request.Id, request.Value);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<FormControlValueResponse> GetByIdAsync(Guid id) => _mapper.Map<FormControlValueResponse>(await _repository.GetAsync(id));
    public async Task<IEnumerable<FormControlValueResponse>> GetAllAsync() => _mapper.Map<IEnumerable<FormControlValueResponse>>(await _repository.GetListAsync());
    public async Task<FormControlValueResponse> GetByValueAsync(string value) =>
        _mapper.Map<FormControlValueResponse>((await _repository.GetListAsync(o => o.Value == value)).FirstOrDefault());
}

