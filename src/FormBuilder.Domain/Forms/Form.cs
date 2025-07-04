using FormBuilder.Domain.Core;
using FormBuilder.Domain.Forms.Exceptions;
using FormBuilder.Domain.FormVersions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.Forms;

public class Form : BaseEntity<Guid>
{
    public string Name { get; private set; }
    public ICollection<FormVersion> Versions { get; set; } = new List<FormVersion>();

    private Form()
    {

    }

    public Form(string name)
    {
        SetName(name);
    }

    public void SetName(string name)
    {
        Name = name ?? throw new FormNameIsNullOrWhiteSpaceException();
    }
}
