using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterUserAsync(RegisterUserDto registerDto);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<UserDto> GetUserByIdAsync(string id);
    Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateDto);
    Task<bool> DeleteUserAsync(string id);
}
