namespace FormBuilder.Models.Exceptions;

public class UserUpdateFailedException : IdentityOperationFailedException
{
    public UserUpdateFailedException() : base("Failed to update user.")
    {
    }

    public UserUpdateFailedException(string message) : base(message)
    {
    }

    public UserUpdateFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
