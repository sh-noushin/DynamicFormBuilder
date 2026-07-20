using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using Microsoft.Extensions.Logging;

namespace FormBuilder.Infrastructure.Services;

// POSTs the submission payload to the form's WebhookUrl and, when a
// WebhookSecret is set, signs the body with HMAC-SHA256.
//
// The receiver verifies by:
//   1. Reading the raw request body as UTF-8 bytes.
//   2. Computing hex(HMAC-SHA256(secret, body)).
//   3. Comparing against the value after "sha256=" in X-Webhook-Signature.
//
// The sender never throws - it swallows failures and logs them so a broken
// receiver cannot break form submission.
public class HttpWebhookSender : IWebhookSender
{
    // Named HttpClient key so the DI setup can register a client with a
    // fixed timeout. Keeps a bad receiver from tying up a request thread.
    public const string HttpClientName = "webhook";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpWebhookSender> _logger;

    public HttpWebhookSender(IHttpClientFactory httpClientFactory, ILogger<HttpWebhookSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(Form form, FormSubmissionDto submission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(form.WebhookUrl)) return;

        try
        {
            byte[] body;
            if (form.WebhookSlackFormat)
            {
                body = BuildSlackPayload(form, submission);
            }
            else
            {
                var payload = new WebhookPayload
                {
                    FormId = form.Id,
                    FormName = form.Name,
                    FormSlug = form.Slug,
                    SubmissionId = submission.Id,
                    SubmittedAt = submission.SubmittedAt,
                    SubmitterName = submission.SubmitterName,
                    SubmitterEmail = submission.SubmitterEmail,
                    Values = submission.Values.ToDictionary(
                        v => v.FieldName ?? string.Empty,
                        v => v.FieldValue)
                };
                body = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
            }
            var request = new HttpRequestMessage(HttpMethod.Post, form.WebhookUrl)
            {
                Content = new ByteArrayContent(body)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.Add("X-Webhook-Event", "form_submission");

            if (!string.IsNullOrWhiteSpace(form.WebhookSecret))
            {
                var signature = ComputeSignature(form.WebhookSecret, body);
                request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");
            }

            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Webhook delivery to {WebhookUrl} for form {FormId} returned {StatusCode}",
                    form.WebhookUrl, form.Id, (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Webhook delivery to {WebhookUrl} for form {FormId} failed",
                form.WebhookUrl, form.Id);
        }
    }

    // Slack incoming-webhook payload shape:
    //   { text, attachments: [ { color, fields: [ { title, value, short } ] } ] }
    // Slack ignores unknown keys, so we omit metadata (formId, submissionId)
    // that the plain JSON payload includes.
    private static byte[] BuildSlackPayload(Form form, FormSubmissionDto submission)
    {
        var attachmentFields = new List<object>();
        if (!string.IsNullOrWhiteSpace(submission.SubmitterName))
            attachmentFields.Add(new { title = "Name", value = submission.SubmitterName, @short = true });
        if (!string.IsNullOrWhiteSpace(submission.SubmitterEmail))
            attachmentFields.Add(new { title = "Email", value = submission.SubmitterEmail, @short = true });
        foreach (var v in submission.Values)
        {
            if (string.IsNullOrWhiteSpace(v.FieldName)) continue;
            attachmentFields.Add(new
            {
                title = v.FieldName,
                value = string.IsNullOrEmpty(v.FieldValue) ? "—" : v.FieldValue,
                @short = (v.FieldValue?.Length ?? 0) < 40,
            });
        }

        var payload = new
        {
            text = $"New submission for *{form.Name}*",
            attachments = new[]
            {
                new
                {
                    color = form.BrandColor ?? "#6366f1",
                    fields = attachmentFields,
                }
            }
        };
        return JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
    }

    private static string ComputeSignature(string secret, byte[] body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(body);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class WebhookPayload
    {
        public Guid FormId { get; set; }
        public string FormName { get; set; } = string.Empty;
        public string FormSlug { get; set; } = string.Empty;
        public Guid SubmissionId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? SubmitterName { get; set; }
        public string? SubmitterEmail { get; set; }
        public Dictionary<string, string?> Values { get; set; } = new();
    }
}
