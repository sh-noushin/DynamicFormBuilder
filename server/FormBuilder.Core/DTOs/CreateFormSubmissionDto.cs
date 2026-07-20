using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class CreateFormSubmissionDto
{
    [Required]
    public Guid FormVersionId { get; set; }

    [StringLength(200)]
    public string? SubmitterName { get; set; }

    [EmailAddress, StringLength(256)]
    public string? SubmitterEmail { get; set; }

    public Dictionary<string, string?> FieldValues { get; set; } = new Dictionary<string, string?>();
}
