using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissions.Exceptions;

public class FormSubmissionFormIdIsNullException : Exception
{
    public FormSubmissionFormIdIsNullException()
        : base("Form submission must have a valid FormId.") { }
}
