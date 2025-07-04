using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Request;

public record UpdateFormSubmissionValueRequest(
      Guid Id,
      string Value
  );
