using FormBuilder.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormControls.Dtos.Request;

public record UpdateFormControlRequest(
      Guid Id,
      string Label,
      ControlType Type,
      bool IsRequired
  );