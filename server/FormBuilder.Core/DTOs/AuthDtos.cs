using System.ComponentModel.DataAnnotations;
using FormBuilder.Models.Entities;

namespace FormBuilder.Core.DTOs;

public class LoginDto
{
    [Required, StringLength(64, MinimumLength = 1)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = new List<UserRole>();
    public string Token { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = new List<UserRole>();
}

public class RegisterUserDto
{
    [Required, StringLength(64, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;
}

public class UpdateUserDto
{
    [Required, StringLength(64, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = new List<UserRole>();
    public DateTime CreatedAt { get; set; }
    // Tenant the user belongs to. Included in the JWT as an "orgId" claim
    // so the service layer can scope queries without another DB lookup.
    public Guid OrganizationId { get; set; }
}

public class ChangePasswordDto
{
    [Required, StringLength(256, MinimumLength = 1)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
}
