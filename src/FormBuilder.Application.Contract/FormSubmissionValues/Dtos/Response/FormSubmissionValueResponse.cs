using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Response;

public record FormSubmissionValueResponse(
        Guid Id,
        Guid FormSubmissionId,
        Guid FormControlId,
        string Value
    );
