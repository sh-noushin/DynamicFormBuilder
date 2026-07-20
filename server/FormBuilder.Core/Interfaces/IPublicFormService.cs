using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IPublicFormService
{
    Task<PublicFormDto> GetBySlugAsync(string slug, string? accessPassword);
    Task<FormSubmissionDto> SubmitAsync(string slug, PublicFormSubmissionDto submission, string? accessPassword);
}
