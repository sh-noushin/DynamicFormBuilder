using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues.Exceptions;

public class FormSubmissionValueSubmissionIdIsNullException : Exception
{
    public FormSubmissionValueSubmissionIdIsNullException()
        : base("Form submission value must be linked to a valid FormSubmission.") { }
}