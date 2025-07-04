using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues.Exceptions;

internal class FormControlValueIsNullOrWhiteSpaceException : Exception
{
    public FormControlValueIsNullOrWhiteSpaceException()
        : base("Form control value cannot be null or empty.") { }
}
