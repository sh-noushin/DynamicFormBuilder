namespace FormBuilder.Models.Exceptions;

public class InvalidRedirectUrlException : InvalidValueException
{
    public InvalidRedirectUrlException()
        : base("Redirect URL must be an absolute http:// or https:// URL.")
    {
    }

    public InvalidRedirectUrlException(string url)
        : base($"Redirect URL '{url}' must be an absolute http:// or https:// URL.")
    {
    }
}
