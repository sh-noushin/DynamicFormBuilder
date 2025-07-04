using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls.Exceptions;
public class FormControlLabelIsNullOrWhiteSpaceException : Exception
{
    public FormControlLabelIsNullOrWhiteSpaceException()
       : base("Form control's Label could not be empty or whitespace.")
    {

    }
}