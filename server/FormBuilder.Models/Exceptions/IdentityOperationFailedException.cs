namespace FormBuilder.Models.Exceptions;

public abstract class IdentityOperationFailedException : Exception
{
    protected IdentityOperationFailedException(string message) : base(message)
    {
    }

    protected IdentityOperationFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
