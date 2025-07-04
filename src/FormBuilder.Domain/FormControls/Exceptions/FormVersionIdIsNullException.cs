using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls.Exceptions;
public class FormVersionIdIsNullException : Exception
{
    public FormVersionIdIsNullException()
       : base("Form control's FormVersionId could not be empty or whitespace.")
    {

    }
}
