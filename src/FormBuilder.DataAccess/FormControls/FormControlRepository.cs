using FormBuilder.DataAccess.Core;
using FormBuilder.Domain.FormControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.DataAccess.FormControls;

public class FormControlRepository
    : BaseRepository<FormBuilderDbContext, FormControl, Guid>, IFormControlRepository
{
    public FormControlRepository(FormBuilderDbContext db) : base(db)
    {
    }
}