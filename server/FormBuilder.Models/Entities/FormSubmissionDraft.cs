namespace FormBuilder.Models.Entities;

// An in-progress submission the visitor saved so they can come back and
// finish later. The Values dictionary is stored as a JSON string in the
// FieldValuesJson column so we don't need a join table; the DTO layer
// materializes it into a dictionary.
//
// Drafts are keyed by ResumeToken (a Guid) which the visitor receives as a
// URL fragment (?draft=<token>). Tokens are effectively unguessable, so the
// "shared link" trust model is the same as an unlisted URL - anyone with the
// link can read + edit the saved values.
public class FormSubmissionDraft
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public Guid ResumeToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public string FieldValuesJson { get; set; } = "{}";
    public Form Form { get; set; } = null!;
}
