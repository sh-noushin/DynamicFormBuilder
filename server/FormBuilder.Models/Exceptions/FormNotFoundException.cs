namespace FormBuilder.Models.Exceptions;

public class FormNotFoundException : EntityNotFoundException
{
    public FormNotFoundException() : base("Form was not found.")
    {
    }

    public FormNotFoundException(Guid formId) : base($"Form with ID '{formId}' was not found.")
    {
    }

    public FormNotFoundException(string message) : base(message)
    {
    }

    public FormNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
