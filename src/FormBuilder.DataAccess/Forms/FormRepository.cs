using FormBuilder.DataAccess.Core;
using FormBuilder.Domain.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.DataAccess.Forms;

public class FormRepository : BaseRepository<FormBuilderDbContext, Form, Guid>, IFormRepository
{
    public FormRepository(FormBuilderDbContext db) : base(db)
    {
    }
}
