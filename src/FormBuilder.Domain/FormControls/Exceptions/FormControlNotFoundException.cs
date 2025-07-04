using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls.Exceptions;
public class FormControlNotFoundException : Exception
{
    public FormControlNotFoundException(string msg)
       : base(msg)
    {

    }
}
