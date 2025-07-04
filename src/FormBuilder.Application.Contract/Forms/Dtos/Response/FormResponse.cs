using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.Forms.Dtos.Response;

public record FormResponse(
      Guid Id,
      string Name
  );