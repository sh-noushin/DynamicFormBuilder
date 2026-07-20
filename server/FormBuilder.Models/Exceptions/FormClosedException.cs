namespace FormBuilder.Models.Exceptions;

// Thrown when a submission is attempted against a form that is no longer
// accepting responses (either the ClosesAt moment has passed or the
// MaxSubmissions cap has been reached).
public class FormClosedException : Exception
{
    public FormClosedException(string reason) : base(reason)
    {
        Reason = reason;
    }

    public string Reason { get; }
}
