namespace FormBuilder.Models.Exceptions;

public class InvalidCurrentPasswordException : Exception
{
    public InvalidCurrentPasswordException() : base("The current password provided is incorrect.")
    {
    }
}
