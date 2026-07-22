using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Options;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormBuilder.Core.Services;

// Password reset via ASP.NET Identity's built-in data-protection tokens.
// No dedicated tokens table — Identity signs the token with the machine
// key so verification is a stateless check. Rely on Identity's default
// token lifetime (~1 day) so we don't have to add expiry logic here.
public class PasswordResetService : IPasswordResetService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _email;
    private readonly NotificationOptions _notifications;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        UserManager<User> userManager,
        IEmailSender email,
        IOptions<NotificationOptions> notifications,
        ILogger<PasswordResetService> logger)
    {
        _userManager = userManager;
        _email = email;
        _notifications = notifications.Value;
        _logger = logger;
    }

    public async Task RequestResetAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return;
        var user = await _userManager.FindByEmailAsync(email);
        // Silent no-op on unknown email — the anonymous endpoint MUST
        // return an identical response regardless of whether the email
        // is registered, so an attacker can't enumerate accounts.
        if (user == null)
        {
            _logger.LogInformation("Password reset requested for unknown email.");
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // URL-encode both fields so the token's slashes / plus signs
        // survive the query string round-trip. The frontend parses them
        // back out on the reset page.
        var resetUrl = $"{_notifications.AppBaseUrl.TrimEnd('/')}/reset-password" +
            $"?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        var subject = "Reset your FormBuilder password";
        var body = $@"Hi,

We received a request to reset the password on your FormBuilder account.

Reset your password: {resetUrl}

If you didn't request this, you can safely ignore this email — your
password will stay the same.

— FormBuilder";

        try
        {
            await _email.SendAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            // Log but don't rethrow — the anonymous caller shouldn't be
            // able to tell whether the send succeeded or failed.
            _logger.LogError(ex, "Failed to send password reset email.");
        }
    }

    public async Task ResetAsync(string email, string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            throw new InvalidCredentialsException("Invalid or expired reset link.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Same wording as bad-token so unknown-email vs bad-token
            // isn't distinguishable from the network.
            throw new InvalidCredentialsException("Invalid or expired reset link.");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidCredentialsException($"Reset failed: {errors}");
        }
    }
}
