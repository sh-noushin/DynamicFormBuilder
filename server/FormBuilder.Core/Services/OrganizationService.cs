using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FormBuilder.Core.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _repo;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtService _jwt;
    private readonly ICurrentUserService _currentUser;

    public OrganizationService(
        IOrganizationRepository repo,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtService jwt,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwt = jwt;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<OrganizationDto>> ListAsync()
    {
        var orgs = await _repo.ListAsync();
        var userCounts = await _repo.GetUserCountsAsync();
        var formCounts = await _repo.GetFormCountsAsync();
        return orgs.Select(o => new OrganizationDto
        {
            Id = o.Id,
            Name = o.Name,
            Slug = o.Slug,
            CreatedAt = o.CreatedAt,
            UserCount = userCounts.GetValueOrDefault(o.Id, 0),
            FormCount = formCounts.GetValueOrDefault(o.Id, 0),
        });
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationDto payload)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrWhiteSpace(payload.Name))
            throw new ArgumentException("Name is required.", nameof(payload));

        var slug = await GenerateUniqueSlugAsync(payload.Name);
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = payload.Name.Trim(),
            Slug = slug,
            CreatedAt = DateTime.UtcNow,
        };
        await _repo.CreateAsync(org);

        // Provision the first tenant admin if all three fields are given.
        // If only some are set, refuse rather than silently creating a
        // half-configured user.
        var anyAdminField = !string.IsNullOrWhiteSpace(payload.AdminUsername)
                         || !string.IsNullOrWhiteSpace(payload.AdminEmail)
                         || !string.IsNullOrWhiteSpace(payload.AdminPassword);
        var allAdminFields = !string.IsNullOrWhiteSpace(payload.AdminUsername)
                          && !string.IsNullOrWhiteSpace(payload.AdminEmail)
                          && !string.IsNullOrWhiteSpace(payload.AdminPassword);
        if (anyAdminField && !allAdminFields)
            throw new ArgumentException("To provision the first admin, provide username, email, AND password.", nameof(payload));

        if (allAdminFields)
        {
            var user = new User
            {
                UserName = payload.AdminUsername,
                Email = payload.AdminEmail,
                OrganizationId = org.Id,
            };
            var createResult = await _userManager.CreateAsync(user, payload.AdminPassword!);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new UserCreationFailedException($"Failed to create tenant admin: {errors}");
            }
            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            await _userManager.AddToRoleAsync(user, Roles.Admin);
        }

        return ToDto(org, userCount: allAdminFields ? 1 : 0, formCount: 0);
    }

    public async Task<OrganizationDto> UpdateAsync(Guid id, UpdateOrganizationDto payload)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        var updated = await _repo.UpdateAsync(id, payload.Name.Trim())
            ?? throw new InvalidOperationException($"Organization '{id}' not found.");
        return ToDto(updated, userCount: 0, formCount: 0);
    }

    public async Task DeleteAsync(Guid id)
    {
        var success = await _repo.DeleteAsync(id);
        if (!success) throw new InvalidOperationException($"Organization '{id}' not found.");
    }

    public async Task<ImpersonationResultDto> ImpersonateAsync(Guid id)
    {
        var org = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Organization '{id}' not found.");

        // Synthetic tenant-admin identity — no real user record needed.
        // The JWT carries Admin role + the target orgId, which is what
        // every downstream tenant filter uses. Sub is a synthetic ID
        // scoped by the org so audit logs can distinguish impersonation
        // from real logins.
        var impersonatedUser = new UserDto
        {
            Id = $"impersonation:{id}",
            Username = $"super@{org.Slug}",
            Email = $"super+{org.Slug}@formbuilder.com",
            OrganizationId = id,
        };
        var token = _jwt.GenerateToken(impersonatedUser, new[] { Roles.Admin });
        return new ImpersonationResultDto
        {
            Token = token,
            OrganizationName = org.Name,
            OrganizationId = id,
        };
    }

    public async Task<OrganizationDto> GetCurrentAsync()
    {
        var orgId = _currentUser.GetOrganizationId();
        var org = await _repo.GetByIdAsync(orgId)
            ?? throw new InvalidOperationException("Workspace not found.");
        return ToDto(org, userCount: 0, formCount: 0);
    }

    public async Task<OrganizationDto> RenameCurrentAsync(UpdateOrganizationDto payload)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        var orgId = _currentUser.GetOrganizationId();
        var updated = await _repo.UpdateAsync(orgId, payload.Name.Trim())
            ?? throw new InvalidOperationException("Workspace not found.");
        return ToDto(updated, userCount: 0, formCount: 0);
    }

    private async Task<string> GenerateUniqueSlugAsync(string name)
    {
        var baseSlug = new string(name.ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "workspace";
        // If the base slug is free, use it; otherwise append a random suffix.
        var existing = await _repo.GetBySlugAsync(baseSlug);
        if (existing == null) return baseSlug;
        return $"{baseSlug}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
    }

    private static OrganizationDto ToDto(Organization o, int userCount, int formCount) => new()
    {
        Id = o.Id,
        Name = o.Name,
        Slug = o.Slug,
        CreatedAt = o.CreatedAt,
        UserCount = userCount,
        FormCount = formCount,
    };
}
