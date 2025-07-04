using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues.Exceptions;

public class FormSubmissionValueControlIdIsNullException : Exception
{
    public FormSubmissionValueControlIdIsNullException()
        : base("Form submission value must be linked to a valid FormControl.") { }
}