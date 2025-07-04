using FormBuilder.Domain.Core;
using FormBuilder.Domain.FormControls;
using FormBuilder.Domain.Forms;
using FormBuilder.Domain.FormVersions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormVersions;

public class FormVersion : BaseEntity<Guid>
{
    public Guid FormId { get; private set; }
    public Form Form { get; private set; } = null!;

    public string Version { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<FormControl> Controls { get; private set; } = new List<FormControl>();

    private FormVersion() { }

    public FormVersion(string version, Guid formId)
    {
        SetVersion(version);
        SetFormId(formId);
    }

    private void SetFormId(Guid formId)
    {
        if (formId == Guid.Empty)
            throw new FormVersionFormIdIsNullException();

        FormId = formId;
    }

    public void SetVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new FormVersionIsNullOrWhiteSpaceException();

        Version = version;
    }
}