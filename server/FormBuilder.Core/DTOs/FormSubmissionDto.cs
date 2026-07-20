namespace FormBuilder.Core.DTOs;

public class FormSubmissionDto
{
    public Guid Id { get; set; }
    public Guid FormVersionId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public string? SubmitterIpAddress { get; set; }
    public string? AdminNotes { get; set; }
    public List<FormSubmissionValueDto> Values { get; set; } = new List<FormSubmissionValueDto>();
}
