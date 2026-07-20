namespace FormBuilder.Models.Entities;

public class Form
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    // Hex color like "#6366f1" used to theme the public /f/:slug page and
    // admin preview. Null falls back to the app-wide indigo palette.
    public string? BrandColor { get; set; }
    // Optional password required to view + submit the public /f/:slug form.
    // Null means anyone with the link can submit.
    public string? AccessPassword { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<FormVersion> Versions { get; set; } = new List<FormVersion>();
}

