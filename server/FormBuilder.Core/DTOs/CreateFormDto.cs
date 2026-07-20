using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class CreateFormDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(9)]
    public string? BrandColor { get; set; }

    [StringLength(200)]
    public string? AccessPassword { get; set; }

    [StringLength(2000)]
    public string? ThankYouMessage { get; set; }

    [StringLength(500)]
    public string? RedirectUrl { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxSubmissions { get; set; }

    public DateTime? ClosesAt { get; set; }

    [StringLength(500)]
    public string? WebhookUrl { get; set; }

    [StringLength(128)]
    public string? WebhookSecret { get; set; }

    public bool WebhookSlackFormat { get; set; }

    public bool OneResponsePerEmail { get; set; }

    public bool OneResponsePerIp { get; set; }

    [StringLength(10)]
    public string? Locale { get; set; }

    [StringLength(500)]
    public string? FaviconUrl { get; set; }

    public bool SendConfirmationEmail { get; set; }

    [StringLength(200)]
    public string? ConfirmationEmailSubject { get; set; }

    [StringLength(4000)]
    public string? ConfirmationEmailBody { get; set; }

    public List<CreateFormFieldDto> Fields { get; set; } = new List<CreateFormFieldDto>();
}
