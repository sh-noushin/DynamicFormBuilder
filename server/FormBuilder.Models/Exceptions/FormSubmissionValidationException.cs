using System.Runtime.Serialization;

namespace FormBuilder.Models.Exceptions;

[Serializable]
public class FormSubmissionValidationException : Exception
{
    public IDictionary<string, IEnumerable<string>> FieldErrors { get; } = new Dictionary<string, IEnumerable<string>>();

    public FormSubmissionValidationException(string message, IDictionary<string, IEnumerable<string>> fieldErrors) : base(message)
    {
        FieldErrors = fieldErrors ?? new Dictionary<string, IEnumerable<string>>();
    }

    protected FormSubmissionValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
