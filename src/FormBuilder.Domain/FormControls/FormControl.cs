using FormBuilder.Domain.Core;
using FormBuilder.Domain.FormControls.Exceptions;
using FormBuilder.Domain.FormControlValues;
using FormBuilder.Domain.FormSubmissionValues;
using FormBuilder.Domain.FormVersions;
using FormBuilder.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls;
public class FormControl : BaseEntity<Guid>
{
    public string Label { get; private set; }
    public ControlType Type { get; private set; }
    public bool IsRequired { get; set; }
    public Guid FormVersionId { get; private set; }
    public FormVersion FormVersion { get; private set; } = null!;

    public ICollection<FormControlValue> Values { get; private set; } = new List<FormControlValue>();
    public ICollection<FormSubmissionValue> SubmissionValues { get; private set; } = new List<FormSubmissionValue>();

    private FormControl() { }

    public FormControl(string label, ControlType type, bool isRequired, Guid formVersionId)
    {
        SetLabel(label);
        SetType(type);
        IsRequired = isRequired;
        SetFormVersionId(formVersionId);
    }

    public bool RequiresOptions => Type == ControlType.Dropdown || Type == ControlType.MultiSelect || Type == ControlType.RadioButton;

    public void SetLabel(string label)
    {
        Label = label ?? throw new FormControlLabelIsNullOrWhiteSpaceException();
    }

    public void SetType(ControlType type)
    {
        if (!Enum.IsDefined(typeof(ControlType), type))
        {
            throw new FormControlTypeIsNullException();
        }
        Type = type;
    }
    public void SetFormVersionId(Guid formVersionId)
    {
        if (formVersionId == Guid.Empty)
            throw new FormVersionIdIsNullException();

        FormVersionId = formVersionId;
    }
}

