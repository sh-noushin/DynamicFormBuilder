namespace FormBuilder.Core.DTOs;

public class UpdateFormSubmissionDto
{
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public Dictionary<string, string?> FieldValues { get; set; } = new Dictionary<string, string?>();
}
