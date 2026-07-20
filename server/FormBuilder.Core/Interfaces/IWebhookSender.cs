using FormBuilder.Core.DTOs;
using FormBuilder.Models.Entities;

namespace FormBuilder.Core.Interfaces;

public interface IWebhookSender
{
    // Delivers a submission to the form's configured webhook URL. No-op when
    // the form has no WebhookUrl set. Implementations MUST NOT throw - a
    // failed delivery must not fail the submission itself. Fire-and-forget
    // semantics are up to the caller.
    Task SendAsync(Form form, FormSubmissionDto submission, CancellationToken cancellationToken = default);
}
