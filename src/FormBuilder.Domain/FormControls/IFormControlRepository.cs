using FormBuilder.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls;

public interface IFormControlRepository : IBaseRepository<FormControl, Guid>
{
}
