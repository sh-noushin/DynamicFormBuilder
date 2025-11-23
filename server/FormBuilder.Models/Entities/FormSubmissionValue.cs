namespace FormBuilder.Models.Entities;

public class FormSubmissionValue
{
    public Guid Id { get; set; }
    public Guid FormSubmissionId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? FieldValue { get; set; }
    public FormSubmission FormSubmission { get; set; } = null!;
}
