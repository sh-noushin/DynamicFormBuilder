namespace FormBuilder.Models.Exceptions;

// Thrown when a submission is rejected because the submitter's email has
// already submitted the same form and OneResponsePerEmail is enabled.
public class DuplicateSubmissionException : Exception
{
    public DuplicateSubmissionException()
        : base("A response has already been submitted from this email address.")
    {
    }
}
