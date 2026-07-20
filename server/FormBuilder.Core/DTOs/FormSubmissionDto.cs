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
    // Tag labels applied by admins. Wire format is a JSON array; the entity
    // stores them CSV-encoded, converted at the mapper/repo boundary.
    public List<string> Tags { get; set; } = new List<string>();
    public List<FormSubmissionValueDto> Values { get; set; } = new List<FormSubmissionValueDto>();
}
