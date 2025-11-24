namespace FormBuilder.Core.DTOs;

public class CreateFormDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CreateFormFieldDto> Fields { get; set; } = new List<CreateFormFieldDto>();
}
