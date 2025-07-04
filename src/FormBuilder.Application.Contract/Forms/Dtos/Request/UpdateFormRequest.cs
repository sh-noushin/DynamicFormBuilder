using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.Forms.Dtos.Request;

public record UpdateFormRequest(
    Guid Id,
    string Name
);
