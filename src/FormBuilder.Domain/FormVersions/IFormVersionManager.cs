using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormVersions;

public interface IFormVersionManager
{
    Task<FormVersion> CreateAsync(string version, Guid formId);
    Task UpdateVersionAsync(Guid id, string newVersion);
    Task DeleteAsync(Guid id);
}