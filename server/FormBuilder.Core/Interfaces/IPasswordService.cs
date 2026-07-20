using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IPasswordService
{
    Task ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
}
