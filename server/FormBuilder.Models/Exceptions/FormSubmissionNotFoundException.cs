namespace FormBuilder.Models.Exceptions;

public class FormSubmissionNotFoundException : EntityNotFoundException
{
    public FormSubmissionNotFoundException() : base("Form submission was not found.")
    {
    }

    public FormSubmissionNotFoundException(Guid formSubmissionId) : base($"Form submission with ID '{formSubmissionId}' was not found.")
    {
    }

    public FormSubmissionNotFoundException(string message) : base(message)
    {
    }

    public FormSubmissionNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
