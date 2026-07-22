namespace FormBuilder.Core.Interfaces;

public interface IPasswordResetService
{
    // Anonymous. Never reveals whether the email is registered so an
    // attacker can't enumerate accounts. Silently no-ops on unknown
    // emails; sends a reset link when the email matches a real user.
    Task RequestResetAsync(string email);

    // Consumes a reset token (issued by RequestResetAsync's email link)
    // and sets the new password. Throws InvalidCredentials on bad token,
    // UserNotFound on unknown email.
    Task ResetAsync(string email, string token, string newPassword);
}
