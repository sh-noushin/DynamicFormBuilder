using FormBuilder.Application.Contract.FormControls.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormVersions.Dtos.Response;

public record FormVersionResponse(
      Guid Id,
      string Version,
      Guid FormId,
      DateTime CreatedAt,
      List<FormControlResponse> Controls
  );
