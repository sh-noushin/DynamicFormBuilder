using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.Core.Exceptions;
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException()
        : base("Entity not found.")
    {

    }
}
