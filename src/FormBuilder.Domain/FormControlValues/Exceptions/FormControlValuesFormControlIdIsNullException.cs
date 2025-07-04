using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues.Exceptions;

public class FormControlValuesFormControlIdIsNullException : Exception
{
    public FormControlValuesFormControlIdIsNullException()
        : base("Form control value must be linked to a valid FormControl.") { }
}
