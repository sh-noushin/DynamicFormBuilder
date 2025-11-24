namespace FormBuilder.Core.DTOs;

public class UpdateFormDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
