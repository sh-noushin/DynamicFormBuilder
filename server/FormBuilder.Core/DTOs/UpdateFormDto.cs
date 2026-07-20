using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class UpdateFormDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(9)]
    public string? BrandColor { get; set; }

    [StringLength(200)]
    public string? AccessPassword { get; set; }

    [StringLength(2000)]
    public string? ThankYouMessage { get; set; }

    [StringLength(500)]
    public string? RedirectUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
