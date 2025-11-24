namespace FormBuilder.Core.DTOs;

public class UpdateFormVersionDto
{
    public string Description { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public List<CreateFormFieldDto> Fields { get; set; } = new List<CreateFormFieldDto>();
}
