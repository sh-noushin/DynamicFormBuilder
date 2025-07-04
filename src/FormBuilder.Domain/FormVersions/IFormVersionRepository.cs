using FormBuilder.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormVersions;

public interface IFormVersionRepository : IBaseRepository<FormVersion, Guid>
{

    Task<List<FormVersion>> GetListWithControlsAsync();

}
