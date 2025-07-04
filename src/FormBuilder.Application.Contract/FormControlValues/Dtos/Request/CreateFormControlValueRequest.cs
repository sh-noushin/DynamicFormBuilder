using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormControlValues.Dtos.Request;

public record CreateFormControlValueRequest(
       string Value,
       Guid FormControlId
   );
