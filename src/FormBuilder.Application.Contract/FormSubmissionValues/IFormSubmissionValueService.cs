using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Request;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormSubmissionValues;

public interface IFormSubmissionValueService
{
    Task<Guid> CreateAsync(CreateFormSubmissionValueRequest request);
    Task UpdateValueAsync(UpdateFormSubmissionValueRequest request);
    Task DeleteAsync(Guid id);
    Task<FormSubmissionValueResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<FormSubmissionValueResponse>> GetAllAsync();
}