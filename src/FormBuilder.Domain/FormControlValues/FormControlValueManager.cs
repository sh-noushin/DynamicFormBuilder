using FormBuilder.Domain.FormControlValues.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControlValues;

public class FormControlValueManager : IFormControlValueManager
{
    private readonly IFormControlValueRepository _repository;

    public FormControlValueManager(IFormControlValueRepository repository)
    {
        _repository = repository;
    }

    public async Task<FormControlValue> CreateAsync(string value, Guid formControlId)
    {
        var formControlOption = new FormControlValue(value, formControlId);
        await _repository.CreateAsync(formControlOption);
        return formControlOption;
    }

    public async Task UpdateAsync(Guid id, string value)
    {
        var formControlOption = await _repository.GetAsync(id);
        if (formControlOption == null)
            throw new FormControlValueNotFoundException($"Form control value with id {id} not found.");

        formControlOption.SetValue(value);

        await _repository.UpdateAsync(formControlOption);
    }

    public async Task DeleteAsync(Guid id)
    {
        var formControlOption = await _repository.GetAsync(id);
        if (formControlOption == null)
            throw new FormControlValueNotFoundException($"Form control value with id {id} not found.");

        await _repository.DeleteAsync(formControlOption);
    }
}
