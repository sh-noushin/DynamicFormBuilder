namespace FormBuilder.Core.DTOs;

public class FormVersionDto
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public int VersionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public bool IsCurrentVersion { get; set; }
    public List<FormFieldDto> Fields { get; set; } = new List<FormFieldDto>();
}
