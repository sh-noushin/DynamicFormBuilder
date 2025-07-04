using FormBuilder.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls;

public interface IFormControlManager
{
    Task<FormControl> CreateAsync(string label, ControlType type, bool isRequired, Guid formVersionId);
    Task UpdateAsync(Guid id, string label, ControlType type, bool isRequired);
    Task DeleteAsync(Guid id);
}
