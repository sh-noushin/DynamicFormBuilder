using AutoMapper;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class FormVersionService : IFormVersionService
{
    private readonly IFormVersionRepository _versionRepository;
    private readonly IFormRepository _formRepository;
    private readonly IMapper _mapper;

    public FormVersionService(IFormVersionRepository versionRepository, IFormRepository formRepository, IMapper mapper)
    {
        _versionRepository = versionRepository;
        _formRepository = formRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FormVersionDto>> GetVersionsByFormIdAsync(Guid formId)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));

        try
        {
            var versions = await _versionRepository.GetVersionsByFormIdAsync(formId);
            return _mapper.Map<IEnumerable<FormVersionDto>>(versions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving versions for form ID {formId}.", ex);
        }
    }

    public async Task<FormVersionDto> GetVersionAsync(Guid formId, int versionNumber)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be greater than zero.", nameof(versionNumber));

        try
        {
            var version = await _versionRepository.GetVersionAsync(formId, versionNumber);
            if (version == null)
                throw new FormVersionNotFoundException($"Version {versionNumber} not found for form ID {formId}.");
            return _mapper.Map<FormVersionDto>(version);
        }
        catch (FormVersionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving version {versionNumber} for form ID {formId}.", ex);
        }
    }

    public async Task<FormVersionDto> GetVersionByIdAsync(Guid versionId)
    {
        if (versionId == Guid.Empty)
            throw new ArgumentException("Version ID cannot be empty.", nameof(versionId));

        try
        {
            var version = await _versionRepository.GetVersionByIdAsync(versionId);
            if (version == null)
                throw new FormVersionNotFoundException(versionId);
            return _mapper.Map<FormVersionDto>(version);
        }
        catch (FormVersionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving version with ID {versionId}.", ex);
        }
    }

    public async Task<FormVersionDto> GetCurrentVersionAsync(Guid formId)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));

        try
        {
            var currentVersion = await _versionRepository.GetCurrentVersionAsync(formId);
            if (currentVersion == null)
                throw new FormVersionNotFoundException($"No current version found for form ID {formId}.");
            return _mapper.Map<FormVersionDto>(currentVersion);
        }
        catch (FormVersionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving current version for form ID {formId}.", ex);
        }
    }

    public async Task<FormVersionDto> CreateVersionAsync(Guid formId, CreateFormVersionDto versionDto)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));
        if (versionDto == null)
            throw new ArgumentNullException(nameof(versionDto), "Version cannot be null.");

        try
        {
            // Verify form exists
            var form = await _formRepository.GetByIdAsync(formId);
            if (form == null)
                throw new FormNotFoundException(formId);

            // Get existing versions to determine next version number
            var existingVersions = await _versionRepository.GetVersionsByFormIdAsync(formId);
            var maxVersionNumber = existingVersions.Any() ? existingVersions.Max(v => v.VersionNumber) : 0;

            // Set up the new version
            var version = _mapper.Map<FormBuilder.Models.Entities.FormVersion>(versionDto);
            version.FormId = formId;
            version.VersionNumber = maxVersionNumber + 1;
            version.CreatedAt = DateTime.UtcNow;
            version.UpdatedAt = DateTime.UtcNow;

            // If this is marked as current version, unset current flag on existing versions
            if (version.IsCurrentVersion)
            {
                var currentVersion = await _versionRepository.GetCurrentVersionAsync(formId);
                if (currentVersion != null)
                {
                    currentVersion.IsCurrentVersion = false;
                    currentVersion.UpdatedAt = DateTime.UtcNow;
                    await _versionRepository.UpdateAsync(formId, currentVersion.VersionNumber, currentVersion);
                }
            }

            var created = await _versionRepository.CreateAsync(version);
            return _mapper.Map<FormVersionDto>(created);
        }
        catch (FormNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while creating version for form ID {formId}.", ex);
        }
    }

    public async Task<FormVersionDto> UpdateVersionAsync(Guid formId, int versionNumber, UpdateFormVersionDto versionDto)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be greater than zero.", nameof(versionNumber));
        if (versionDto == null)
            throw new ArgumentNullException(nameof(versionDto), "Version cannot be null.");

        try
        {
            var entity = _mapper.Map<FormBuilder.Models.Entities.FormVersion>(versionDto);
            entity.UpdatedAt = DateTime.UtcNow;
            var updatedVersion = await _versionRepository.UpdateAsync(formId, versionNumber, entity);
            if (updatedVersion == null)
                throw new FormVersionNotFoundException($"Version {versionNumber} not found for form ID {formId}.");
            return _mapper.Map<FormVersionDto>(updatedVersion);
        }
        catch (FormVersionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating version {versionNumber} for form ID {formId}.", ex);
        }
    }

    public async Task<bool> DeleteVersionAsync(Guid formId, int versionNumber)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be greater than zero.", nameof(versionNumber));

        try
        {
            var result = await _versionRepository.DeleteAsync(formId, versionNumber);
            if (!result)
                throw new FormVersionNotFoundException($"Version {versionNumber} not found for form ID {formId}.");
            return result;
        }
        catch (FormVersionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting version {versionNumber} for form ID {formId}.", ex);
        }
    }

    public async Task<bool> PublishVersionAsync(Guid formId, int versionNumber)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be greater than zero.", nameof(versionNumber));

        try
        {
            var version = await _versionRepository.GetVersionAsync(formId, versionNumber);
            if (version == null)
                throw new FormVersionNotFoundException($"Version {versionNumber} not found for form ID {formId}.");
            version.IsPublished = true;
            version.UpdatedAt = DateTime.UtcNow;
            var result = await _versionRepository.UpdateAsync(formId, versionNumber, version);
            return result != null;
        }
        catch (FormVersionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while publishing version {versionNumber} for form ID {formId}.", ex);
        }
    }

    public async Task<bool> SetCurrentVersionAsync(Guid formId, int versionNumber)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be greater than zero.", nameof(versionNumber));

        try
        {
            // First, unset current flag on all versions
            var allVersions = await _versionRepository.GetVersionsByFormIdAsync(formId);
            foreach (var ver in allVersions.Where(v => v.IsCurrentVersion))
            {
                ver.IsCurrentVersion = false;
                ver.UpdatedAt = DateTime.UtcNow;
                await _versionRepository.UpdateAsync(formId, ver.VersionNumber, ver);
            }

            // Set the target version as current
            var targetVersion = await _versionRepository.GetVersionAsync(formId, versionNumber);
            if (targetVersion == null)
                throw new FormVersionNotFoundException($"Version {versionNumber} not found for form ID {formId}.");
            targetVersion.IsCurrentVersion = true;
            targetVersion.UpdatedAt = DateTime.UtcNow;
            var result = await _versionRepository.UpdateAsync(formId, versionNumber, targetVersion);
            return result != null;
        }
        catch (FormVersionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while setting version {versionNumber} as current for form ID {formId}.", ex);
        }
    }
}
