using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class ApiKeyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // First few chars of the raw key, always in plaintext so admins can
    // pick a specific key out of the list without seeing the full secret.
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; }
    // Tenant the key belongs to. Populated on ValidateAsync so the
    // ApiKey auth handler can put an orgId claim on the resulting
    // ClaimsPrincipal — that's what downstream tenant filtering uses.
    public Guid OrganizationId { get; set; }
}

// Extends ApiKeyDto with the raw key value - returned ONCE at mint time.
// Subsequent GETs never include it because the DB never stores it.
public class ApiKeyCreatedDto : ApiKeyDto
{
    public string Key { get; set; } = string.Empty;
}

public class CreateApiKeyDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}
