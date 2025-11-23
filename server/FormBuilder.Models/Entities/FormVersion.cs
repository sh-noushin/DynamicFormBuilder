namespace FormBuilder.Models.Entities;

public class FormVersion
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public int VersionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public bool IsCurrentVersion { get; set; }
    public Form Form { get; set; } = null!;
    public List<FormVersionField> Fields { get; set; } = new List<FormVersionField>();
}
