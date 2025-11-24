using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(UserDto user, IList<string> roles);
}
