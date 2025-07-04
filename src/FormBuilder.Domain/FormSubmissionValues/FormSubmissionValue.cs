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
    public Guid FormSubmissionId { get; private set; }
    public FormSubmission FormSubmission { get; private set; } = null!;

    public Guid FormControlId { get; private set; }
    public FormControl FormControl { get; private set; } = null!;

    public string Value { get; private set; }

    private FormSubmissionValue() { }

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
