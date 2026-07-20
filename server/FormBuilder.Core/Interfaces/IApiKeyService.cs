using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IApiKeyService
{
    Task<IEnumerable<ApiKeyDto>> ListAsync();
    // Mints a fresh key. The returned DTO includes the raw Key exactly once;
    // the caller MUST surface it to the admin because it cannot be recovered
    // later.
    Task<ApiKeyCreatedDto> CreateAsync(CreateApiKeyDto payload);
    Task<ApiKeyDto> RevokeAsync(Guid id);
    // Server-side validation used by the auth handler: hashes the raw key,
    // looks up, filters out revoked. Returns null on any failure.
    Task<ApiKeyDto?> ValidateAsync(string rawKey);
}
