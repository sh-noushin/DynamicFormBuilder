namespace FormBuilder.Models.Entities;

public class Form
{
    public Guid Id { get; set; }
    // Tenant the form belongs to. Every authenticated API query filters
    // on this; public /f/:slug access finds the form by globally-unique
    // slug and doesn't need to know the caller's org.
    public Guid OrganizationId { get; set; }
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
    // When true, the webhook body is reshaped into Slack incoming-webhook
    // JSON (text + attachment fields) so the admin can wire a webhook URL
    // straight to a Slack channel without any glue code.
    public bool WebhookSlackFormat { get; set; }
    // When true, a submission is rejected if the submitter's email address
    // has already submitted this form (checked case-insensitively across all
    // versions). Anonymous submissions - those without an email - always pass.
    public bool OneResponsePerEmail { get; set; }
    // Same rule as OneResponsePerEmail but keyed off the submitter's IP.
    // Anti-abuse companion for public surveys that don't collect email.
    public bool OneResponsePerIp { get; set; }
    // BCP 47 locale code (e.g. "en", "es", "fr") for the public form UI
    // strings. Null falls back to English. Only client-side rendering uses
    // this - the server never translates messages itself.
    public string? Locale { get; set; }
    // http/https URL to a favicon that overrides the tab icon on the public
    // /f/:slug page. Null keeps the app-wide default. Validated in the
    // service layer with the same http(s)-only rule as RedirectUrl.
    public string? FaviconUrl { get; set; }
    // When true, the submitter receives an email copy of their submission
    // after a successful submit (only if they provided an email address).
    public bool SendConfirmationEmail { get; set; }
    // Optional per-form overrides for the confirmation subject and body.
    // Null falls back to the default template.
    public string? ConfirmationEmailSubject { get; set; }
    public string? ConfirmationEmailBody { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<FormVersion> Versions { get; set; } = new List<FormVersion>();
}

