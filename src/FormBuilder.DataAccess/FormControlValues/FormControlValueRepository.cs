using FormBuilder.DataAccess.Core;
using FormBuilder.Domain.FormControlValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.DataAccess.FormControlValues;

public class FormControlValueRepository
    : BaseRepository<FormBuilderDbContext, FormControlValue, Guid>, IFormControlValueRepository
{
    public FormControlValueRepository(FormBuilderDbContext db) : base(db)
    {
    }
}
