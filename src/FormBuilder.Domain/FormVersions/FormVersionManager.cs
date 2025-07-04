using FormBuilder.Domain.FormVersions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormBuilder.Domain.FormVersions;

public class FormVersionManager : IFormVersionManager
{
    private readonly IFormVersionRepository _repository;

    public FormVersionManager(IFormVersionRepository repository)
    {
        _repository = repository;
    }

    public async Task<FormVersion> CreateAsync(string version, Guid formId)
    {
        var formVersion = new FormVersion(version, formId);
        await _repository.CreateAsync(formVersion);
        return formVersion;
    }

    public async Task UpdateVersionAsync(Guid id, string newVersion)
    {
        var formVersion = await _repository.GetAsync(id);
        if (formVersion == null)
            throw new FormVersionNotFoundException($"Form version with ID {id} not found.");

        formVersion.SetVersion(newVersion);

        await _repository.UpdateAsync(formVersion);
    }

    public async Task DeleteAsync(Guid id)
    {
        var formVersion = await _repository.GetAsync(id);
        if (formVersion == null)
            throw new FormVersionNotFoundException($"Form version with ID {id} not found.");

        await _repository.DeleteAsync(formVersion);
    }
}
