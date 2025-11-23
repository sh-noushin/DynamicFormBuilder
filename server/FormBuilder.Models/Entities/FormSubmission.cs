namespace FormBuilder.Models.Entities;

public class FormSubmission
{
    public Guid Id { get; set; }
    public Guid FormVersionId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public string? SubmitterIpAddress { get; set; }
    public FormVersion FormVersion { get; set; } = null!;
    public List<FormSubmissionValue> Values { get; set; } = new List<FormSubmissionValue>();
}
