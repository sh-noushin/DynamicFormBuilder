namespace FormBuilder.Core.DTOs;

// Wire shape for draft save + resume. Values dictionary matches the shape
// used by the public submit endpoint so the client can round-trip the same
// object without transformation.
public class SaveDraftDto
{
    // When present, the server updates the existing draft with this token
    // (and refreshes ExpiresAt). When absent, the server creates a new draft
    // and returns a fresh token.
    public Guid? ResumeToken { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string? SubmitterName { get; set; }

    [System.ComponentModel.DataAnnotations.EmailAddress, System.ComponentModel.DataAnnotations.StringLength(200)]
    public string? SubmitterEmail { get; set; }

    public Dictionary<string, string?> FieldValues { get; set; } = new Dictionary<string, string?>();
}

public class FormSubmissionDraftDto
{
    public Guid ResumeToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public Dictionary<string, string?> FieldValues { get; set; } = new Dictionary<string, string?>();
}
