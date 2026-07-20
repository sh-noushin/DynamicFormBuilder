using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace FormBuilder.Core.Services;

public class PasswordService : IPasswordService
{
    private readonly UserManager<User> _userManager;

    public PasswordService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        if (changePasswordDto == null)
            throw new ArgumentNullException(nameof(changePasswordDto), "Change password data cannot be null.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UserNotFoundException(userId);

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        if (result.Succeeded)
            return;

        if (result.Errors.Any(error =>
            string.Equals(error.Code, nameof(IdentityErrorDescriber.PasswordMismatch), StringComparison.OrdinalIgnoreCase) ||
            error.Description.Contains("incorrect", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidCurrentPasswordException();
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new PasswordChangeFailedException(errors);
    }
}
