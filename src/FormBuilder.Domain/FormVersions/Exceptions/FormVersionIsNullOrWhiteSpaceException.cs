using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormVersions.Exceptions;

public class FormVersionIsNullOrWhiteSpaceException : Exception
{
    public FormVersionIsNullOrWhiteSpaceException()
       : base("Form version could not be empty or whitespace.")
    {

    }
}
