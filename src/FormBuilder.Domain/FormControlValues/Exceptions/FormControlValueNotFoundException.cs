using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues.Exceptions;

public class FormControlValueNotFoundException : Exception
{
    public FormControlValueNotFoundException(string msg)
        : base(msg) { }
}
