using FormBuilder.Domain.FormControls.Exceptions;
using FormBuilder.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormControls;

public class FormControlManager : IFormControlManager
{
    private readonly IFormControlRepository _repository;

    public FormControlManager(IFormControlRepository repository)
    {
        _repository = repository;
    }

    public async Task<FormControl> CreateAsync(string label, ControlType type, bool isRequired, Guid formVersionId)
    {
        var formControl = new FormControl(label, type, isRequired, formVersionId);
        await _repository.CreateAsync(formControl);
        return formControl;
    }

    public async Task UpdateAsync(Guid id, string label, ControlType type, bool isRequired)
    {
        var formControl = await _repository.GetAsync(id);
        if (formControl == null)
            throw new FormControlNotFoundException($"Form control with id {id} not found.");

        formControl.SetLabel(label);
        formControl.SetType(type);
        formControl.IsRequired = isRequired;

        await _repository.UpdateAsync(formControl);
    }

    public async Task DeleteAsync(Guid id)
    {
        var formControl = await _repository.GetAsync(id);
        if (formControl == null)
            throw new FormControlNotFoundException($"Form control with id {id} not found.");

        await _repository.DeleteAsync(formControl);
    }
}
