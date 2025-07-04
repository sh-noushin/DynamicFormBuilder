using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues.Exceptions;

public class FormSubmissionValueIsNullOrWhiteSpaceException : Exception
{
    public FormSubmissionValueIsNullOrWhiteSpaceException()
        : base("Form submission value cannot be null or empty.") { }
}
