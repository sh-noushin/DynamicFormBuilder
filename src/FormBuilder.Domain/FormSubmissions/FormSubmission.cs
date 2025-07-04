using FormBuilder.Domain.Core;
using FormBuilder.Domain.FormSubmissions.Exceptions;
using FormBuilder.Domain.FormSubmissionValues;
using FormBuilder.Domain.FormVersions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissions;

public class FormSubmission : BaseEntity<Guid>
{
    //public Guid FormId { get; private set; }
    //public Form Form { get; private set; }  // <- nullable navigation

    public Guid FormVersionId { get; private set; }
    public FormVersion FormVersion { get; private set; }  // <- nullable navigation

    public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<FormSubmissionValue> Values { get; private set; } = new List<FormSubmissionValue>();

    private FormSubmission() { }

    public FormSubmission(Guid formId, Guid formVersionId)
    {
        SetFormId(formId);
        SetFormVersionId(formVersionId);
        SubmittedAt = DateTime.UtcNow;
    }

    public void SetFormId(Guid formId)
    {
        if (formId == Guid.Empty)
            throw new FormSubmissionFormIdIsNullException();

        //FormId = formId;
    }

    public void SetFormVersionId(Guid formVersionId)
    {
        if (formVersionId == Guid.Empty)
            throw new FormSubmissionFormVersionIdIsNullException();

        FormVersionId = formVersionId;
    }

    public void AddValue(Guid formControlId, string value)
    {
        if (formControlId == Guid.Empty)
            throw new FormSubmissionInvalidControlException("Form control ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(value))
            throw new FormSubmissionInvalidValueException("Submission value cannot be empty.");

        if (Values.Any(v => v.FormControlId == formControlId))
            throw new FormSubmissionDuplicateValueException($"A value for FormControlId {formControlId} already exists.");

        Values.Add(new FormSubmissionValue(Id, formControlId, value));
    }
}
