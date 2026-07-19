using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IPublicFormService
{
    Task<PublicFormDto> GetBySlugAsync(string slug);
    Task<FormSubmissionDto> SubmitAsync(string slug, PublicFormSubmissionDto submission);
}
