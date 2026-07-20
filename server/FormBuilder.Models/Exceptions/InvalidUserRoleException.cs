namespace FormBuilder.Models.Exceptions;

public class InvalidUserRoleException : InvalidValueException
{
    public InvalidUserRoleException() : base("Invalid user role specified.")
    {
    }

    public InvalidUserRoleException(string userRole) : base($"User role '{userRole}' is not valid.")
    {
    }

    public InvalidUserRoleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
