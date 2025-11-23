namespace FormBuilder.Models.Exceptions;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException() : base("Email already exists.")
    {
    }

    public DuplicateEmailException(string email) : base($"Email '{email}' already exists.")
    {
    }

    public DuplicateEmailException(string message, Exception innerException) : base(message, innerException)
    {
    }
}