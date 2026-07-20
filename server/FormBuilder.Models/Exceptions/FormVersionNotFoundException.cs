namespace FormBuilder.Models.Exceptions;

public class FormVersionNotFoundException : EntityNotFoundException
{
    public FormVersionNotFoundException() : base("Form version was not found.")
    {
    }

    public FormVersionNotFoundException(Guid formVersionId) : base($"Form version with ID '{formVersionId}' was not found.")
    {
    }

    public FormVersionNotFoundException(string message) : base(message)
    {
    }

    public FormVersionNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
