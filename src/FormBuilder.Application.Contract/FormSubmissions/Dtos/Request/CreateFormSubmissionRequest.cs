using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormSubmissions.Dtos.Request;

public record CreateFormSubmissionRequest(
        Guid FormId,
        Guid FormVersionId,
        List<CreateFormSubmissionValueRequest> Values
  );