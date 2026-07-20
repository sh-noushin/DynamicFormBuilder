namespace FormBuilder.Models.Exceptions;

public class PasswordChangeFailedException : IdentityOperationFailedException
{
    public PasswordChangeFailedException(string message) : base(message)
    {
    }
}
