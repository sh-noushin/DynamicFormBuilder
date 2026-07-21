using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IUserService
{
    // Authenticated caller (tenant admin) creating a user inside their
    // own workspace. Anonymous callers can't reach this — public
    // signup goes through RegisterInTenantAsync instead.
    Task<UserDto> RegisterUserAsync(RegisterUserDto registerDto);
    // Anonymous public sign-up into an existing tenant identified by
    // its slug. Always creates the new user with the User role.
    Task<UserDto> RegisterInTenantAsync(string tenantSlug, RegisterUserDto registerDto);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<UserDto> GetUserByIdAsync(string id);
    Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateDto);
    Task<bool> DeleteUserAsync(string id);
}
