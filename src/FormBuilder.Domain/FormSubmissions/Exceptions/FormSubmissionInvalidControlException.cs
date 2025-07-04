using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissions.Exceptions;

public class FormSubmissionInvalidControlException : Exception
{
    public FormSubmissionInvalidControlException(string message) : base(message) { }
}
