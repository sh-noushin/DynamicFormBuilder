namespace FormBuilder.Models.Exceptions;

// Thrown when a tenant tries to perform an action their plan doesn't
// cover — e.g. creating a form beyond the Free tier's cap. The API
// middleware turns this into HTTP 402 Payment Required with a body
// pointing the caller to the billing page.
public class PlanLimitExceededException : Exception
{
    public string LimitName { get; }

    public PlanLimitExceededException(string limitName, string message) : base(message)
    {
        LimitName = limitName;
    }
}
