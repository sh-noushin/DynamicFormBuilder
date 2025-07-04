using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormControlValues.Dtos.Response;

public record FormControlValueResponse(
       Guid Id,
       string Value,
       Guid FormControlId
   );
