using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissions.Exceptions;

public class FormSubmissionFormVersionIdIsNullException : Exception
{
    public FormSubmissionFormVersionIdIsNullException()
        : base("Form submission must have a valid FormVersionId.") { }
}
