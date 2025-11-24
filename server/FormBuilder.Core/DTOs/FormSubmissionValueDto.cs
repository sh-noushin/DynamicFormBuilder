namespace FormBuilder.Core.DTOs;

public class FormSubmissionValueDto
{
    public Guid Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? FieldValue { get; set; }
}
