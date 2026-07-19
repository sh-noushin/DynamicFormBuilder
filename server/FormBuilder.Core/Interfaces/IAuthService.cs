using System.Security.Claims;
using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginDto loginDto);
    Task LogoutAsync();
    Task<UserInfoDto> GetCurrentUserAsync(ClaimsPrincipal principal);
}
