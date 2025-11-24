using FormBuilder.Models.Entities;

namespace FormBuilder.Core.DTOs;

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
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
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User; 
public class UpdateUserDto
{
    public string Username { get; set; } = string.Empty;
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
}
