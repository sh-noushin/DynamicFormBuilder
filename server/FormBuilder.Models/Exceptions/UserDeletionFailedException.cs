namespace FormBuilder.Models.Exceptions;

public class UserDeletionFailedException : IdentityOperationFailedException
{
    public UserDeletionFailedException() : base("Failed to delete user.")
    {
    }

    public UserDeletionFailedException(string message) : base(message)
    {
    }

    public UserDeletionFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
