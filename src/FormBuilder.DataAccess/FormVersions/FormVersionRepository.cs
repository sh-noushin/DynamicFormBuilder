using FormBuilder.DataAccess.Core;
using FormBuilder.Domain.FormVersions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.DataAccess.FormVersions;

public class FormVersionRepository : BaseRepository<FormBuilderDbContext, FormVersion, Guid>, IFormVersionRepository
{
    public FormVersionRepository(FormBuilderDbContext db) : base(db)
    {
    }
    public async Task<List<FormVersion>> GetListWithControlsAsync()
    {
        return await Db.FormVersions
            .Include(fv => fv.Controls)
                .ThenInclude(fc => fc.Values)
            .ToListAsync();
    }
}
