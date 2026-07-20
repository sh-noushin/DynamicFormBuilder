namespace FormBuilder.Models.Exceptions;

// Thrown when a submission's hidden honeypot field is non-empty - a strong
// signal that a bot filled it in, since real users never see or touch it.
public class HoneypotTriggeredException : Exception
{
    public HoneypotTriggeredException()
        : base("Submission blocked.")
    {
    }
}
