using System.Text;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Options;
using FormBuilder.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormBuilder.Core.Services;

public class SubmitterConfirmationSender : ISubmitterConfirmationSender
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<SubmitterConfirmationSender> _logger;

    public SubmitterConfirmationSender(
        IEmailSender emailSender,
        IOptions<NotificationOptions> options,
        ILogger<SubmitterConfirmationSender> logger)
    {
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(Form form, FormSubmissionDto submission, CancellationToken cancellationToken = default)
    {
        if (!form.SendConfirmationEmail) return;
        if (string.IsNullOrWhiteSpace(submission.SubmitterEmail)) return;

        // Unlike the admin notifier, we don't need AdminEmail to be set - we
        // just need a working SMTP host. Skip silently in dev when SMTP is
        // not configured so form submissions still work end-to-end.
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogInformation(
                "Submitter confirmation skipped (SMTP not configured): form={FormName} submissionId={SubmissionId}",
                form.Name, submission.Id);
            return;
        }

        var subject = string.IsNullOrWhiteSpace(form.ConfirmationEmailSubject)
            ? $"Thanks for your response to {form.Name}"
            : form.ConfirmationEmailSubject!;

        var body = BuildBody(form, submission);

        try
        {
            await _emailSender.SendAsync(submission.SubmitterEmail!, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send submitter confirmation for form {FormName} to {Submitter}",
                form.Name, submission.SubmitterEmail);
        }
    }

    // Body layout:
    //   <admin's custom prefix, or default>
    //
    //   Submitted at: <iso>
    //
    //   Your response:
    //     field: value
    //     ...
    private static string BuildBody(Form form, FormSubmissionDto submission)
    {
        var sb = new StringBuilder();

        var prefix = string.IsNullOrWhiteSpace(form.ConfirmationEmailBody)
            ? $"Thanks for your response to '{form.Name}'. A copy of what you submitted is below for your records."
            : form.ConfirmationEmailBody!;

        sb.AppendLine(prefix);
        sb.AppendLine();
        sb.AppendLine($"Submitted at: {submission.SubmittedAt:u}");
        sb.AppendLine();
        sb.AppendLine("Your response:");
        foreach (var value in submission.Values.OrderBy(v => v.FieldName))
        {
            sb.AppendLine($"  {value.FieldName}: {value.FieldValue}");
        }
        return sb.ToString();
    }
}
