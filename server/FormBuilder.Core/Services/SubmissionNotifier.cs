using System.Text;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormBuilder.Core.Services;

public class SubmissionNotifier : ISubmissionNotifier
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<SubmissionNotifier> _logger;

    public SubmissionNotifier(
        IEmailSender emailSender,
        IOptions<NotificationOptions> options,
        ILogger<SubmissionNotifier> logger)
    {
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task NotifyAsync(string formName, FormSubmissionDto submission, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogInformation(
                "Notification skipped (SMTP not configured): form={FormName} submissionId={SubmissionId} submitter={Submitter}",
                formName, submission.Id, submission.SubmitterEmail ?? "(anonymous)");
            return;
        }

        var subject = $"New submission: {formName}";
        var body = BuildBody(formName, submission);

        try
        {
            await _emailSender.SendAsync(_options.AdminEmail, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            // Never fail a submission because the notification failed.
            _logger.LogError(ex, "Failed to send submission notification for form {FormName}", formName);
        }
    }

    private static string BuildBody(string formName, FormSubmissionDto submission)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"A new submission was received for '{formName}'.");
        sb.AppendLine();
        sb.AppendLine($"Submitted at: {submission.SubmittedAt:u}");
        if (!string.IsNullOrWhiteSpace(submission.SubmitterName))
            sb.AppendLine($"Name: {submission.SubmitterName}");
        if (!string.IsNullOrWhiteSpace(submission.SubmitterEmail))
            sb.AppendLine($"Email: {submission.SubmitterEmail}");
        sb.AppendLine();
        sb.AppendLine("Fields:");
        foreach (var value in submission.Values.OrderBy(v => v.FieldName))
        {
            sb.AppendLine($"  {value.FieldName}: {value.FieldValue}");
        }
        return sb.ToString();
    }
}
