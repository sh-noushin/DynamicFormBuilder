using FormBuilder.DataAccess.Core;
using FormBuilder.Domain.FormSubmissionValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.DataAccess.FormSubmissionValues;

public class FormSubmissionValueRepository
   : BaseRepository<FormBuilderDbContext, FormSubmissionValue, Guid>, IFormSubmissionValueRepository
{
    public FormSubmissionValueRepository(FormBuilderDbContext db) : base(db)
    {
    }
}
