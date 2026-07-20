namespace FormBuilder.Core.DTOs;

// Portable snapshot of a form's definition, safe to download + re-upload
// on another instance. Deliberately omits ids, timestamps, slug, submission
// history, and secrets (AccessPassword / WebhookSecret) so exports can be
// checked into git or emailed without exposing sensitive state.
public class FormExportDto
{
    // Bump on breaking schema changes so ImportFormAsync can refuse
    // unknown-shape payloads with a clear error.
    public int FormatVersion { get; set; } = 1;
    public DateTime ExportedAt { get; set; }
    public FormExportBodyDto Form { get; set; } = new();
}

public class FormExportBodyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BrandColor { get; set; }
    public string? ThankYouMessage { get; set; }
    public string? RedirectUrl { get; set; }
    public int? MaxSubmissions { get; set; }
    public DateTime? ClosesAt { get; set; }
    public string? WebhookUrl { get; set; }
    public bool OneResponsePerEmail { get; set; }
    public bool SendConfirmationEmail { get; set; }
    public string? ConfirmationEmailSubject { get; set; }
    public string? ConfirmationEmailBody { get; set; }
    public List<FormExportFieldDto> Fields { get; set; } = new();
}

public class FormExportFieldDto
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    // Kept as a string here so callers using the export as a black box
    // don't have to marshal the FieldType enum themselves. Import parses
    // this back through Enum.TryParse.
    public string Type { get; set; } = "Text";
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsReadOnly { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DefaultValue { get; set; }
    public string? Validation { get; set; }
    public string? Options { get; set; }
    public string? ShowIfCondition { get; set; }
}
