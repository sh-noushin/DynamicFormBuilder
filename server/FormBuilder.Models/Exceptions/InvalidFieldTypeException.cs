namespace FormBuilder.Models.Exceptions;

public class InvalidFieldTypeException : InvalidValueException
{
    public InvalidFieldTypeException() : base("Invalid field type specified.")
    {
    }

    public InvalidFieldTypeException(string fieldType) : base($"Field type '{fieldType}' is not valid.")
    {
    }

    public InvalidFieldTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
