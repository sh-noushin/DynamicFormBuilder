namespace FormBuilder.Models.Exceptions;

public abstract class InvalidValueException : Exception
{
    protected InvalidValueException(string message) : base(message)
    {
    }

    protected InvalidValueException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
