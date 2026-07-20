using FormBuilder.Core.DTOs;
using FormBuilder.Models.Entities;

namespace FormBuilder.Core.Interfaces;

public interface ISubmitterConfirmationSender
{
    // Sends a copy of the submission to the submitter's email address.
    // No-op when the form has SendConfirmationEmail=false, when the submitter
    // did not provide an email, or when SMTP is not configured.
    // Implementations MUST NOT throw - a failed confirmation must never
    // fail the submission itself.
    Task SendAsync(Form form, FormSubmissionDto submission, CancellationToken cancellationToken = default);
}
