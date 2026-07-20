namespace FormBuilder.Models.Entities;

public class Form
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    // Hex color like "#6366f1" used to theme the public /f/:slug page and
    // admin preview. Null falls back to the app-wide indigo palette.
    public string? BrandColor { get; set; }
    // Optional password required to view + submit the public /f/:slug form.
    // Null means anyone with the link can submit.
    public string? AccessPassword { get; set; }
    // Custom message shown on the public form after a successful submission.
    // If RedirectUrl is set, this is ignored.
    public string? ThankYouMessage { get; set; }
    // External URL to redirect to after a successful submission. Must be
    // absolute and use http/https - validated in the service layer.
    public string? RedirectUrl { get; set; }
    // Total submissions allowed across all versions. Null means uncapped.
    public int? MaxSubmissions { get; set; }
    // UTC moment after which the form stops accepting responses. Null means
    // no scheduled close.
    public DateTime? ClosesAt { get; set; }
    // Absolute http/https URL that receives a POST after each successful
    // submission. Validated by FormService.
    public string? WebhookUrl { get; set; }
    // Shared secret used to compute an HMAC-SHA256 signature over the
    // webhook body, sent as X-Webhook-Signature. Null means unsigned.
    public string? WebhookSecret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<FormVersion> Versions { get; set; } = new List<FormVersion>();
}

