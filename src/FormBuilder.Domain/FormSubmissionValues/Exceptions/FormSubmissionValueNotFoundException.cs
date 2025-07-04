using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues.Exceptions;

public class FormSubmissionValueNotFoundException : Exception
{
    public FormSubmissionValueNotFoundException(string msg)
        : base(msg) { }
}
