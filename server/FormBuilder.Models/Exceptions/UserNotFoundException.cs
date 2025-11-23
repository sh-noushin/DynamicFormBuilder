namespace FormBuilder.Models.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException() : base("User was not found.")
    {
    }

    public UserNotFoundException(string userId) : base($"User with ID '{userId}' was not found.")
    {
    }

    public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
