namespace FormBuilder.Models.Exceptions;

public class PasswordChangeFailedException : Exception
{
    public PasswordChangeFailedException(string message) : base(message)
    {
    }
}
