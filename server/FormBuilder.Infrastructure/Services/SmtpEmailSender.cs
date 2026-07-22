using System.Net;
using System.Net.Mail;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormBuilder.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly NotificationOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<NotificationOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            // Dev fallback — log the entire email so the developer can
            // grab links out of it (password reset, submission confirmations).
            _logger.LogInformation(
                "SmtpEmailSender.SendAsync no-op (SMTP not configured).\n  to: {To}\n  subject: {Subject}\n  body:\n{Body}",
                toAddress, subject, body);
            return;
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpUseSsl,
            Credentials = string.IsNullOrEmpty(_options.SmtpUsername)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toAddress);

        await client.SendMailAsync(message, cancellationToken);
    }
}
