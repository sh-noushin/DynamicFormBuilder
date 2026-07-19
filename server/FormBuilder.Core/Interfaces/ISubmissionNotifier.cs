using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface ISubmissionNotifier
{
    Task NotifyAsync(string formName, FormSubmissionDto submission, CancellationToken cancellationToken = default);
}
