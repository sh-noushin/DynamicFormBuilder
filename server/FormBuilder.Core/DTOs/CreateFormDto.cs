using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class CreateFormDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public List<CreateFormFieldDto> Fields { get; set; } = new List<CreateFormFieldDto>();
}
