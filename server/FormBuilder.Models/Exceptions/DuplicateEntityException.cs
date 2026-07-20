namespace FormBuilder.Models.Exceptions;

public abstract class DuplicateEntityException : Exception
{
    protected DuplicateEntityException(string message) : base(message)
    {
    }

    protected DuplicateEntityException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
