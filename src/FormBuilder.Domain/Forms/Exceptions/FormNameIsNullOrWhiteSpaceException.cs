using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.Forms.Exceptions;

public class FormNameIsNullOrWhiteSpaceException : Exception
{
    public FormNameIsNullOrWhiteSpaceException()
       : base("Form name could not be empty or whitespace.")
    {

    }
}
