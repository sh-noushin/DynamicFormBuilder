namespace FormBuilder.Models.Exceptions;

public class FormSubmissionValueNotFoundException : EntityNotFoundException
{
    public FormSubmissionValueNotFoundException() : base("Form submission value was not found.")
    {
    }

    public FormSubmissionValueNotFoundException(Guid formSubmissionValueId) : base($"Form submission value with ID '{formSubmissionValueId}' was not found.")
    {
    }

    public FormSubmissionValueNotFoundException(string fieldName) : base($"Form submission value for field '{fieldName}' was not found.")
    {
    }

    public FormSubmissionValueNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
