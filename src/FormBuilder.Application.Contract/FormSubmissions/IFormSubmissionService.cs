using FormBuilder.Application.Contract.FormSubmissions.Dtos.Request;
using FormBuilder.Application.Contract.FormSubmissions.Dtos.Response;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormSubmissions;

public interface IFormSubmissionService
{
    Task<Guid> CreateAsync(CreateFormSubmissionRequest request);
    Task AddValueAsync(Guid submissionId, CreateFormSubmissionValueRequest value);
    Task DeleteAsync(Guid id);
    Task<FormSubmissionResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<FormSubmissionResponse>> GetAllAsync();

}