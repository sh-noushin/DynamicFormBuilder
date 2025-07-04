using FormBuilder.Domain.Core;
using FormBuilder.Domain.FormControls;
using FormBuilder.Domain.FormControlValues.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues;

public class FormControlValue : BaseEntity<Guid>
{
    public string Value { get; private set; }
    public Guid FormControlId { get; private set; }
    public FormControl FormControl { get; private set; } = null!;

    private FormControlValue() { }

    public FormControlValue(string value, Guid formControlId)
    {
        SetValue(value);
        SetFormControlId(formControlId);
    }

    public void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FormControlValueIsNullOrWhiteSpaceException();

        Value = value;
    }


    public void SetFormControlId(Guid formControlId)
    {
        if (formControlId == Guid.Empty)
            throw new FormControlValuesFormControlIdIsNullException();

        FormControlId = formControlId;
    }
}
