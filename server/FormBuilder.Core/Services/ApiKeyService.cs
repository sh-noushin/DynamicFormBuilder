using System.Security.Cryptography;
using System.Text;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class ApiKeyService : IApiKeyService
{
    // "fbk_" = form builder key. Grep-friendly prefix so a leaked key stands
    // out in logs and admins can pattern-match it in receiver code.
    private const string KeyPrefix = "fbk_";
    // 32 hex chars = 128 bits of entropy. Enough that brute-force lookup
    // is not a realistic threat even with a large keys table.
    private const int RandomHexChars = 32;
    // How many chars (including the "fbk_" prefix) we surface in the
    // KeyPrefix column so admins can identify keys in the list.
    private const int DisplayPrefixLength = 8;

    private readonly IApiKeyRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public ApiKeyService(IApiKeyRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ApiKeyDto>> ListAsync()
    {
        var keys = await _repository.GetAllAsync();
        return keys.Select(ToDto);
    }

    public async Task<ApiKeyCreatedDto> CreateAsync(CreateApiKeyDto payload)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrWhiteSpace(payload.Name))
            throw new ArgumentException("API key name is required.", nameof(payload));

        var rawKey = GenerateKey();
        var entity = new ApiKey
        {
            // Key is minted under the calling admin's tenant so callers
            // presenting the key are treated as acting inside that org.
            OrganizationId = _currentUser.GetOrganizationId(),
            Name = payload.Name.Trim(),
            KeyPrefix = rawKey.Substring(0, DisplayPrefixLength),
            KeyHash = Hash(rawKey),
            CreatedAt = DateTime.UtcNow,
        };
        var created = await _repository.CreateAsync(entity);

        var dto = ToDto(created);
        return new ApiKeyCreatedDto
        {
            Id = dto.Id,
            Name = dto.Name,
            KeyPrefix = dto.KeyPrefix,
            CreatedAt = dto.CreatedAt,
            LastUsedAt = dto.LastUsedAt,
            IsRevoked = dto.IsRevoked,
            Key = rawKey,
        };
    }

    public async Task<ApiKeyDto> RevokeAsync(Guid id)
    {
        var revoked = await _repository.RevokeAsync(id)
            ?? throw new ApiKeyNotFoundException(id);
        return ToDto(revoked);
    }

    public async Task<ApiKeyDto?> ValidateAsync(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey)) return null;
        if (!rawKey.StartsWith(KeyPrefix, StringComparison.Ordinal)) return null;

        var hash = Hash(rawKey);
        var key = await _repository.GetByHashAsync(hash);
        if (key == null) return null;
        if (key.RevokedAt.HasValue) return null;

        // Fire-and-forget LastUsedAt update. We don't await it so per-request
        // auth stays snappy even against a slow DB, but the caller can pin
        // this down further if that ever becomes a concern.
        _ = _repository.TouchLastUsedAsync(key.Id, DateTime.UtcNow);
        return ToDto(key);
    }

    private static string GenerateKey()
    {
        // 16 bytes -> 32 hex chars. Cryptographically random.
        var bytes = RandomNumberGenerator.GetBytes(RandomHexChars / 2);
        return KeyPrefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Hash(string rawKey)
    {
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static ApiKeyDto ToDto(ApiKey key) => new()
    {
        Id = key.Id,
        Name = key.Name,
        KeyPrefix = key.KeyPrefix,
        CreatedAt = key.CreatedAt,
        LastUsedAt = key.LastUsedAt,
        IsRevoked = key.RevokedAt.HasValue,
        OrganizationId = key.OrganizationId,
    };
}
