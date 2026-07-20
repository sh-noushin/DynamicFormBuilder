using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IApiKeyRepository
{
    Task<IEnumerable<ApiKey>> GetAllAsync();
    Task<ApiKey?> GetByHashAsync(string keyHash);
    Task<ApiKey> CreateAsync(ApiKey apiKey);
    Task<ApiKey?> RevokeAsync(Guid id);
    // Fire-and-forget from the auth handler on a successful match; a failed
    // touch is not surfaced so it can never fail an otherwise-good request.
    Task TouchLastUsedAsync(Guid id, DateTime timestamp);
}
