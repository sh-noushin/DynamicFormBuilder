using Microsoft.AspNetCore.Identity;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;

namespace FormBuilder.Core.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserDto registerDto)
    {
        if (registerDto == null)
            throw new ArgumentNullException(nameof(registerDto), "Register data cannot be null.");

        var existingUser = await _userManager.FindByNameAsync(registerDto.Username);
        if (existingUser != null)
            throw new DuplicateUsernameException(registerDto.Username);

        var existingEmailUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingEmailUser != null)
            throw new DuplicateEmailException(registerDto.Email);

        var user = new User
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserCreationFailedException($"Failed to create user: {errors}");
        }

        await AssignRoleAsync(user, registerDto.Role.ToString());

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = ConvertStringRolesToEnumRoles(roles),
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = ConvertStringRolesToEnumRoles(roles),
                CreatedAt = DateTime.UtcNow
            });
        }
        return userDtos;
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            throw new UserNotFoundException(id);
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = ConvertStringRolesToEnumRoles(roles),
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateDto)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto), "Update data cannot be null.");

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            throw new UserNotFoundException(id);

        if (!string.IsNullOrEmpty(updateDto.Username) && updateDto.Username != user.UserName)
        {
            var existingUser = await _userManager.FindByNameAsync(updateDto.Username);
            if (existingUser != null)
                throw new DuplicateUsernameException(updateDto.Username);
            user.UserName = updateDto.Username;
        }

        if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
        {
            var existingEmailUser = await _userManager.FindByEmailAsync(updateDto.Email);
            if (existingEmailUser != null)
                throw new DuplicateEmailException(updateDto.Email);
            user.Email = updateDto.Email;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new UserUpdateFailedException($"Failed to update user: {errors}");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeRolesResult.Succeeded)
            throw new UserUpdateFailedException("Failed to remove existing roles from user");

        await AssignRoleAsync(user, updateDto.Role.ToString());

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = ConvertStringRolesToEnumRoles(roles),
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            throw new UserNotFoundException(id);
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserDeletionFailedException($"Failed to delete user: {errors}");
        }
        return true;
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

    private async Task AssignRoleAsync(User user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
                throw new InvalidUserRoleException($"Failed to create role '{roleName}'");
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!addRoleResult.Succeeded)
            throw new InvalidUserRoleException($"Failed to assign role '{roleName}' to user");
    }

    private static List<UserRole> ConvertStringRolesToEnumRoles(IList<string> stringRoles)
    {
        var enumRoles = new List<UserRole>();
        foreach (var role in stringRoles)
        {
            if (Enum.TryParse<UserRole>(role, out var enumRole))
                enumRoles.Add(enumRole);
        }
        return enumRoles;
    }
}
