using System.Security.Claims;
using AutoMapper;
using FormBuilder.Core.Common;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FormBuilder.Core.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IOrganizationRepository _orgs;

    public AuthService(UserManager<User> userManager, IJwtService jwtService, IMapper mapper, IOrganizationRepository orgs)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _mapper = mapper;
        _orgs = orgs;
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto loginDto)
    {
        if (loginDto == null)
            throw new ArgumentNullException(nameof(loginDto));

        var user = await _userManager.FindByNameAsync(loginDto.Username);
        if (user == null)
            throw new InvalidCredentialsException();

        var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
            throw new InvalidCredentialsException();

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserDto>(user);
        var token = _jwtService.GenerateToken(userDto, roles);
        var org = await _orgs.GetByIdAsync(user.OrganizationId);

        return new LoginResultDto
        {
            Success = true,
            Username = userDto.Username,
            Email = userDto.Email,
            Roles = RoleMapper.ToEnumRoles(roles),
            Token = token,
            OrganizationName = org?.Name ?? string.Empty,
        };
    }

    // With a JWT-based auth flow, logout is client-side (drop the token). This
    // exists so the controller can keep a consistent surface for cookie-based
    // sessions if they are ever re-enabled.
    public Task LogoutAsync() => Task.CompletedTask;

    public async Task<UserInfoDto> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userName = principal.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            throw new InvalidCredentialsException();

        // Impersonation tokens carry a synthetic NameIdentifier
        // ("impersonation:{orgId}") and don't correspond to a real user
        // in the Identity store. Look up the target tenant by orgId
        // claim and return a synthetic UserInfoDto instead of failing
        // with UserNotFound.
        var nameId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(nameId) && nameId.StartsWith("impersonation:"))
        {
            var orgIdClaim = principal.FindFirst("orgId")?.Value;
            Organization? impersonatedOrg = null;
            if (Guid.TryParse(orgIdClaim, out var impersonatedOrgId))
            {
                impersonatedOrg = await _orgs.GetByIdAsync(impersonatedOrgId);
            }
            var impersonatedRoles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return new UserInfoDto
            {
                Username = userName,
                Email = string.Empty,
                Roles = RoleMapper.ToEnumRoles(impersonatedRoles),
                OrganizationName = impersonatedOrg != null
                    ? $"[Impersonating] {impersonatedOrg.Name}"
                    : string.Empty,
            };
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            throw new UserNotFoundException(userName);

        var roles = await _userManager.GetRolesAsync(user);
        var org = await _orgs.GetByIdAsync(user.OrganizationId);
        return new UserInfoDto
        {
            Username = user.UserName!,
            Email = user.Email!,
            Roles = RoleMapper.ToEnumRoles(roles),
            OrganizationName = org?.Name ?? string.Empty,
        };
    }
}
