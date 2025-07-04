using FormBuilder.Domain.Core;
using FormBuilder.Domain.FormControls;
using FormBuilder.Domain.FormSubmissions;
using FormBuilder.Domain.FormSubmissionValues.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues;

public class FormSubmissionValue : BaseEntity<Guid>
{
    public Guid FormSubmissionId { get;  set; }
    public FormSubmission FormSubmission { get;  set; } = null!;

    public Guid FormControlId { get;  set; }
    public FormControl FormControl { get;  set; } = null!;

    public string Value { get;  set; }

    public FormSubmissionValue() { }

    public FormSubmissionValue(Guid formSubmissionId, Guid formControlId, string value)
    {
        SetFormSubmissionId(formSubmissionId);
        SetFormControlId(formControlId);
        SetValue(value);
    }

    private void SetFormSubmissionId(Guid formSubmissionId)
    {
        if (formSubmissionId == Guid.Empty)
            throw new FormSubmissionValueSubmissionIdIsNullException();

        FormSubmissionId = formSubmissionId;
    }

    private void SetFormControlId(Guid formControlId)
    {
        if (formControlId == Guid.Empty)
            throw new FormSubmissionValueControlIdIsNullException();

        FormControlId = formControlId;
    }

    public void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FormSubmissionValueIsNullOrWhiteSpaceException();

        Value = value;
    }
}
