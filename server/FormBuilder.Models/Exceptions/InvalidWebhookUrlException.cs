namespace FormBuilder.Models.Exceptions;

public class InvalidWebhookUrlException : InvalidValueException
{
    public InvalidWebhookUrlException()
        : base("Webhook URL must be an absolute http:// or https:// URL.")
    {
    }

    public InvalidWebhookUrlException(string url)
        : base($"Webhook URL '{url}' must be an absolute http:// or https:// URL.")
    {
    }
}
