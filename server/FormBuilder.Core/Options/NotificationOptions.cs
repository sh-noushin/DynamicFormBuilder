namespace FormBuilder.Core.Options;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    // If empty or null, submission emails are logged instead of sent (dev fallback).
    public string AdminEmail { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "no-reply@formbuilder.local";
    public string FromDisplayName { get; set; } = "FormBuilder";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SmtpHost) && !string.IsNullOrWhiteSpace(AdminEmail);
}
