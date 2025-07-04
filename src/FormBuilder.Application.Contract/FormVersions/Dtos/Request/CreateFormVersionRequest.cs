using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Application.Contract.FormVersions.Dtos.Request;

public record CreateFormVersionRequest(
     string Version,
     Guid FormId
 );