namespace FormBuilder.Core.DTOs;

public class CreateFormVersionDto
{
    public string Description { get; set; } = string.Empty;
    public List<CreateFormFieldDto> Fields { get; set; } = new List<CreateFormFieldDto>();
}
