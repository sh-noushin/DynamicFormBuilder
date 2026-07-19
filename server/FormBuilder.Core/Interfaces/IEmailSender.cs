namespace FormBuilder.Core.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default);
}
