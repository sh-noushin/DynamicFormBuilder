using Microsoft.AspNetCore.Identity;
using FormBuilder.Core.Common;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IOrganizationRepository _orgs;

    public UserService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ICurrentUserService currentUser,
        IOrganizationRepository orgs)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUser = currentUser;
        _orgs = orgs;
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

        // Two call sites, one endpoint: an authenticated admin creating a
        // teammate reuses their org; an anonymous self-signup spins up a
        // fresh org (customer creating an account for their company).
        var callerOrgId = _currentUser.GetOrganizationIdOrNull();
        var (organizationId, isSelfSignup) = callerOrgId != null
            ? (callerOrgId.Value, false)
            : (await CreateOrganizationForSignupAsync(registerDto.Username), true);

        var user = new User
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
            OrganizationId = organizationId,
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserCreationFailedException($"Failed to create user: {errors}");
        }

        // Self-signup users are always the admin of the org they just
        // created — otherwise there'd be nobody who could manage forms.
        var effectiveRole = isSelfSignup ? UserRole.Admin.ToString() : registerDto.Role.ToString();
        await AssignRoleAsync(user, effectiveRole);

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = RoleMapper.ToEnumRoles(roles),
            CreatedAt = DateTime.UtcNow,
            OrganizationId = user.OrganizationId,
        };
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        // Tenant scope: admins only see users inside their own org.
        // Identity's UserManager.Users bypasses the DbContext query
        // filters we set up for Form/ApiKey, so filter explicitly here.
        var orgId = _currentUser.GetOrganizationId();
        var users = _userManager.Users.Where(u => u.OrganizationId == orgId).ToList();
        var userIdToRoles = await BuildUserRoleMapAsync();

        return users.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = RoleMapper.ToEnumRoles(userIdToRoles.GetValueOrDefault(user.Id, new List<string>())),
            CreatedAt = DateTime.UtcNow,
            OrganizationId = user.OrganizationId,
        }).ToList();
    }

    private async Task<Dictionary<string, List<string>>> BuildUserRoleMapAsync()
    {
        var allRoles = _roleManager.Roles.ToList();
        var map = new Dictionary<string, List<string>>();

        foreach (var role in allRoles)
        {
            if (string.IsNullOrEmpty(role.Name))
                continue;

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            foreach (var user in usersInRole)
            {
                if (!map.TryGetValue(user.Id, out var roles))
                {
                    roles = new List<string>();
                    map[user.Id] = roles;
                }
                roles.Add(role.Name);
            }
        }

        return map;
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));

        var user = await _userManager.FindByIdAsync(id);
        // Treat cross-tenant access as "not found" so probing another
        // org's user IDs doesn't leak existence.
        if (user == null || user.OrganizationId != _currentUser.GetOrganizationId())
            throw new UserNotFoundException(id);
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = RoleMapper.ToEnumRoles(roles),
            CreatedAt = DateTime.UtcNow,
            OrganizationId = user.OrganizationId,
        };
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateDto)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto), "Update data cannot be null.");

        var user = await _userManager.FindByIdAsync(id);
        if (user == null || user.OrganizationId != _currentUser.GetOrganizationId())
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
            Roles = RoleMapper.ToEnumRoles(roles),
            CreatedAt = DateTime.UtcNow,
            OrganizationId = user.OrganizationId,
        };
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));

        var user = await _userManager.FindByIdAsync(id);
        if (user == null || user.OrganizationId != _currentUser.GetOrganizationId())
            throw new UserNotFoundException(id);
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserDeletionFailedException($"Failed to delete user: {errors}");
        }
        return true;
    }

    // Spins up a new tenant for a self-registered admin. The org's Name
    // defaults to the username's workspace ("alice's Workspace") — the
    // admin can rename it later. Slug is derived from the username but
    // guaranteed unique by appending a short random suffix.
    private async Task<Guid> CreateOrganizationForSignupAsync(string username)
    {
        var baseSlug = new string((username ?? "workspace").ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "workspace";
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        var slug = $"{baseSlug}-{suffix}";
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = $"{username}'s Workspace",
            Slug = slug,
            CreatedAt = DateTime.UtcNow,
        };
        var created = await _orgs.CreateAsync(org);
        return created.Id;
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

}
