using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues.Exceptions;

public class FormControlValueDisplayTextIsNullOrWhiteSpaceException : Exception
{
    public FormControlValueDisplayTextIsNullOrWhiteSpaceException()
        : base("Form control option display text cannot be null or empty.") { }
}
