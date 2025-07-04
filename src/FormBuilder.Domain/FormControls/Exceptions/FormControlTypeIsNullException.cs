using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls.Exceptions;
public class FormControlTypeIsNullException : Exception
{
    public FormControlTypeIsNullException()
       : base("Form control's Type  could not be empty or whitespace.")
    {

    }
}
