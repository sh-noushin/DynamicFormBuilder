using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormVersions.Exceptions;

public class FormVersionFormIdIsNullException : Exception
{
    public FormVersionFormIdIsNullException()
        : base("Form version must be linked to a valid FormId.") { }
}
