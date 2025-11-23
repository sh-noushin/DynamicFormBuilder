namespace FormBuilder.Models.Exceptions;

public class FormVersionFieldNotFoundException : Exception
{
    public FormVersionFieldNotFoundException() : base("Form version field was not found.")
    {
    }

    public FormVersionFieldNotFoundException(Guid formVersionFieldId) : base($"Form version field with ID '{formVersionFieldId}' was not found.")
    {
    }

    public FormVersionFieldNotFoundException(string fieldName) : base($"Form version field with name '{fieldName}' was not found.")
    {
    }

    public FormVersionFieldNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
