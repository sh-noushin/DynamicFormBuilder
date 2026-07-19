using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(UserDto user, IList<string> roles);
}
