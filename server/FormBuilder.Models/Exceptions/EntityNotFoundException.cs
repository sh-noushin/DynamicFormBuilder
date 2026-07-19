namespace FormBuilder.Models.Exceptions;

public abstract class EntityNotFoundException : Exception
{
    protected EntityNotFoundException(string message) : base(message)
    {
    }

    protected EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
