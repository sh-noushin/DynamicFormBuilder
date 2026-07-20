using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

// Trimmed public representation of a published form. Intentionally omits
// versioning, IsActive, and any admin-facing detail so the shape can be handed
// to an unauthenticated caller without leaking internal state.
public class PublicFormDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BrandColor { get; set; }
    public string? ThankYouMessage { get; set; }
    public string? RedirectUrl { get; set; }
    // True when the form has an AccessPassword and the caller has not yet
    // supplied it. When true, Fields is empty and FormVersionId is Guid.Empty.
    public bool RequiresPassword { get; set; }
    public Guid FormVersionId { get; set; }
    public int VersionNumber { get; set; }
    public List<FormFieldDto> Fields { get; set; } = new List<FormFieldDto>();
}

// Submission payload sent from the public /f/:slug page. Deliberately does not
// carry FormVersionId - the server resolves it from the slug so a malicious
// caller cannot submit against an arbitrary version.
public class PublicFormSubmissionDto
{
    [StringLength(200)]
    public string? SubmitterName { get; set; }

    [EmailAddress, StringLength(256)]
    public string? SubmitterEmail { get; set; }

    public Dictionary<string, string?> FieldValues { get; set; } = new Dictionary<string, string?>();
}
