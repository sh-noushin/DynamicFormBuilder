using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormSubmissions.Dtos.Response;

public record FormSubmissionResponse(
       Guid Id,
       Guid FormId,
       Guid FormVersionId,
       DateTime SubmittedAt,
       List<FormSubmissionValueResponse> Values
   );