using FormBuilder.Domain.FormSubmissionValues.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormSubmissionValues;

public class FormSubmissionValueManager : IFormSubmissionValueManager
{
    private readonly IFormSubmissionValueRepository _repository;

    public FormSubmissionValueManager(IFormSubmissionValueRepository repository)
    {
        _repository = repository;
    }

    public async Task<FormSubmissionValue> CreateAsync(Guid formSubmissionId, Guid formControlId, string value)
    {
        var submissionValue = new FormSubmissionValue(formSubmissionId, formControlId, value);
        await _repository.CreateAsync(submissionValue);
        return submissionValue;
    }

    public async Task UpdateValueAsync(Guid id, string newValue)
    {
        var submissionValue = await _repository.GetAsync(id);
        if (submissionValue == null)
            throw new FormSubmissionValueNotFoundException($"Form submissionValue with ID {id} not found.");

        submissionValue.SetValue(newValue);

        await _repository.UpdateAsync(submissionValue);
    }

    public async Task DeleteAsync(Guid id)
    {
        var submissionValue = await _repository.GetAsync(id);
        if (submissionValue == null)
            throw new FormSubmissionValueNotFoundException($"Form submissionValue with ID {id} not found.");

        await _repository.DeleteAsync(submissionValue);
    }
}
