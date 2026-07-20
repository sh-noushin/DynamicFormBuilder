namespace FormBuilder.Core.DTOs;

public class FormDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? BrandColor { get; set; }
    public string? AccessPassword { get; set; }
    public string? ThankYouMessage { get; set; }
    public string? RedirectUrl { get; set; }
    public int? MaxSubmissions { get; set; }
    public DateTime? ClosesAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<FormVersionDto> Versions { get; set; } = new List<FormVersionDto>();
    public FormVersionDto? CurrentVersion { get; set; }
}
