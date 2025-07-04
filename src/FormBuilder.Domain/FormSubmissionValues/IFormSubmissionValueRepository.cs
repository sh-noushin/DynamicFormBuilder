using FormBuilder.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues;

public interface IFormSubmissionValueRepository : IBaseRepository<FormSubmissionValue, Guid>
{
}
