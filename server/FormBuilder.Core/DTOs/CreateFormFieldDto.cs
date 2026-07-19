using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class CreateFormFieldDto
{
    public Guid FormVersionId { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string Label { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 1)]
    public string Type { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = false;
    public string? Validation { get; set; }
    public string? DefaultValue { get; set; }
    public string? Options { get; set; }

    [StringLength(200)]
    public string? Placeholder { get; set; }

    [StringLength(500)]
    public string? HelpText { get; set; }

    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsReadOnly { get; set; } = false;
}
