namespace FormBuilder.Models.Exceptions;

public class UserCreationFailedException : IdentityOperationFailedException
{
    public UserCreationFailedException() : base("Failed to create user.")
    {
    }

    public UserCreationFailedException(string message) : base(message)
    {
    }

    public UserCreationFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
