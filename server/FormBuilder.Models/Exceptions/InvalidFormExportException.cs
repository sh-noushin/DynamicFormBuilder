namespace FormBuilder.Models.Exceptions;

// Thrown when an import payload doesn't match the current export schema.
// Maps to HTTP 400 via the existing InvalidValueException handling.
public class InvalidFormExportException : InvalidValueException
{
    public InvalidFormExportException(string message) : base(message) { }
}
