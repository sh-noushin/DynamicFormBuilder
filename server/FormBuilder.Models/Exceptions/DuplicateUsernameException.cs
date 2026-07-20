namespace FormBuilder.Models.Exceptions;

public class DuplicateUsernameException : DuplicateEntityException
{
    public DuplicateUsernameException() : base("Username already exists.")
    {
    }

    public DuplicateUsernameException(string username) : base($"Username '{username}' already exists.")
    {
    }

    public DuplicateUsernameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
