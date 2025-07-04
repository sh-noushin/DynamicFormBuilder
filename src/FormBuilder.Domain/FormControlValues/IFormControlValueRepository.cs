using FormBuilder.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues;

public interface IFormControlValueRepository : IBaseRepository<FormControlValue, Guid>
{
}
