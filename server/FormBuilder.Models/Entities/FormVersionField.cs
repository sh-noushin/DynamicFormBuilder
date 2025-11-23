namespace FormBuilder.Models.Entities;

public class FormVersionField
{
    public Guid Id { get; set; }
    public Guid FormVersionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType Type { get; set; }
    public bool IsRequired { get; set; }
    public string? Validation { get; set; }
    public string? DefaultValue { get; set; }
    public string? Options { get; set; } 
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsReadOnly { get; set; } = false;
    public FormVersion FormVersion { get; set; } = null!;
}