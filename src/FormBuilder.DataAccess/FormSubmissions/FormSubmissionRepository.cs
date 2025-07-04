using FormBuilder.DataAccess.Core;
using FormBuilder.Domain.FormSubmissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.DataAccess.FormSubmissions;

public class FormSubmissionRepository
    : BaseRepository<FormBuilderDbContext, FormSubmission, Guid>, IFormSubmissionRepository
{
    public FormSubmissionRepository(FormBuilderDbContext db) : base(db)
    {
    }
}
