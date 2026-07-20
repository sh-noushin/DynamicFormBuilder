namespace FormBuilder.Models.Entities;

// Admin-issued API key for external integrations (Zapier / n8n / a custom
// script). The raw key value is returned exactly ONCE at mint time; the DB
// only stores its SHA-256 hash so a compromised database cannot be used to
// impersonate integrations.
//
// Keys carry the "Admin" role for authorization purposes, matching the
// behavior a scripting user would expect from an API token issued by an
// admin - callers are trusted the same as the admin that minted the key.
public class ApiKey
{
    public Guid Id { get; set; }
    // Human-friendly label shown in the admin UI ("Zapier integration").
    public string Name { get; set; } = string.Empty;
    // First few chars of the raw key, kept in plaintext so admins can
    // identify keys in the list without the DB storing the full token.
    public string KeyPrefix { get; set; } = string.Empty;
    // SHA-256 hex of the raw key. Server compares by hashing the incoming
    // header value and looking this up.
    public string KeyHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    // Non-null RevokedAt means the key has been disabled. Rows are kept
    // (not deleted) for audit trail purposes.
    public DateTime? RevokedAt { get; set; }
}
