using FormBuilder.Domain.FormVersions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.Forms;

public interface IFormManager
{
    Task<Form> CreateAsync(string name);
    Task UpdateAsync(Guid id, string name);
    Task DeleteAsync(Guid id);
    Task<FormVersion> AddVersionAsync(Guid formId, string version);
}
