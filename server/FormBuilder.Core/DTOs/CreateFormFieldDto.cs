namespace FormBuilder.Core.DTOs;

public class CreateFormFieldDto
{
    public Guid FormVersionId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public string? Validation { get; set; }
    public string? DefaultValue { get; set; }
    public string? Options { get; set; } 
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsReadOnly { get; set; } = false;
}
