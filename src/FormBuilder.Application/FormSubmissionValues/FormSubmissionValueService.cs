using AutoMapper;
using FormBuilder.Application.Contract.FormSubmissionValues;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Request;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Response;
using FormBuilder.Domain.FormSubmissionValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.FormSubmissionValues;

public class FormSubmissionValueService : IFormSubmissionValueService
{
    private readonly IFormSubmissionValueManager _manager;
    private readonly IFormSubmissionValueRepository _repository;
    private readonly IMapper _mapper;

    public FormSubmissionValueService(
        IFormSubmissionValueManager manager,
        IFormSubmissionValueRepository repository,
        IMapper mapper)
    {
        _manager = manager;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateFormSubmissionValueRequest request)
    {
        var entity = await _manager.CreateAsync(request.FormSubmissionId, request.FormControlId, request.Value);
        return entity.Id;
    }

    public async Task UpdateValueAsync(UpdateFormSubmissionValueRequest request)
    {
        await _manager.UpdateValueAsync(request.Id, request.Value);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<FormSubmissionValueResponse> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return _mapper.Map<FormSubmissionValueResponse>(entity);
    }

    public async Task<IEnumerable<FormSubmissionValueResponse>> GetAllAsync()
    {
        var list = await _repository.GetListAsync();
        return _mapper.Map<IEnumerable<FormSubmissionValueResponse>>(list);
    }
}
