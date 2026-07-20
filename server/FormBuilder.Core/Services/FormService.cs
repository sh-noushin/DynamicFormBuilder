using AutoMapper;
using FormBuilder.Core.Common;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class FormService : IFormService
{
    private readonly IFormRepository _formRepository;
    private readonly IMapper _mapper;

    public FormService(IFormRepository formRepository, IMapper mapper)
    {
        _formRepository = formRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FormDto>> GetAllFormsAsync()
    {
        var forms = await _formRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<FormDto>>(forms);
    }

    public async Task<FormDto> GetFormByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var form = await _formRepository.GetByIdAsync(id);
        if (form == null)
            throw new FormNotFoundException(id);

        return _mapper.Map<FormDto>(form);
    }

    public async Task<FormDto> CreateFormAsync(CreateFormDto formDto)
    {
        if (formDto == null)
            throw new ArgumentNullException(nameof(formDto), "Form cannot be null.");

        ValidateRedirectUrl(formDto.RedirectUrl);
        ValidateWebhookUrl(formDto.WebhookUrl);
        var entity = _mapper.Map<FormBuilder.Models.Entities.Form>(formDto);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Slug = SlugGenerator.Generate();
        foreach (var version in entity.Versions)
        {
            version.CreatedAt = DateTime.UtcNow;
            version.UpdatedAt = DateTime.UtcNow;
        }
        var created = await _formRepository.CreateAsync(entity);
        return _mapper.Map<FormDto>(created);
    }

    public async Task<FormDto> UpdateFormAsync(Guid id, UpdateFormDto formDto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));
        if (formDto == null)
            throw new ArgumentNullException(nameof(formDto), "Form cannot be null.");

        ValidateRedirectUrl(formDto.RedirectUrl);
        ValidateWebhookUrl(formDto.WebhookUrl);
        var entity = _mapper.Map<FormBuilder.Models.Entities.Form>(formDto);
        entity.UpdatedAt = DateTime.UtcNow;
        var updatedForm = await _formRepository.UpdateAsync(id, entity);
        if (updatedForm == null)
            throw new FormNotFoundException(id);
        return _mapper.Map<FormDto>(updatedForm);
    }

    public async Task<bool> DeleteFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var result = await _formRepository.DeleteAsync(id);
        if (!result)
            throw new FormNotFoundException(id);
        return result;
    }

    public async Task<bool> ActivateFormAsync(Guid id) => await SetFormActiveStateAsync(id, true);

    public async Task<bool> DeactivateFormAsync(Guid id) => await SetFormActiveStateAsync(id, false);

    public async Task<FormDto> DuplicateFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var source = await _formRepository.GetByIdAsync(id);
        if (source == null)
            throw new FormNotFoundException(id);

        var now = DateTime.UtcNow;
        var currentVersion = source.Versions.FirstOrDefault(v => v.IsCurrentVersion) ?? source.Versions.LastOrDefault();

        var clone = new FormBuilder.Models.Entities.Form
        {
            Name = $"Copy of {source.Name}",
            Description = source.Description,
            BrandColor = source.BrandColor,
            Slug = SlugGenerator.Generate(),
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now,
            Versions = new List<FormBuilder.Models.Entities.FormVersion>()
        };

        if (currentVersion != null)
        {
            var newVersion = new FormBuilder.Models.Entities.FormVersion
            {
                VersionNumber = 1,
                Description = currentVersion.Description,
                CreatedAt = now,
                UpdatedAt = now,
                IsPublished = false,
                IsCurrentVersion = true,
                Fields = currentVersion.Fields
                    .OrderBy(f => f.Order)
                    .Select(f => new FormBuilder.Models.Entities.FormVersionField
                    {
                        Name = f.Name,
                        Label = f.Label,
                        Type = f.Type,
                        IsRequired = f.IsRequired,
                        Validation = f.Validation,
                        DefaultValue = f.DefaultValue,
                        Options = f.Options,
                        Placeholder = f.Placeholder,
                        HelpText = f.HelpText,
                        ShowIfCondition = f.ShowIfCondition,
                        Order = f.Order,
                        IsVisible = f.IsVisible,
                        IsReadOnly = f.IsReadOnly
                    })
                    .ToList()
            };
            clone.Versions.Add(newVersion);
        }

        var created = await _formRepository.CreateAsync(clone);
        return _mapper.Map<FormDto>(created);
    }

    public async Task<FormExportDto> ExportFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var source = await _formRepository.GetByIdAsync(id)
            ?? throw new FormNotFoundException(id);

        var version = source.Versions.FirstOrDefault(v => v.IsCurrentVersion)
                      ?? source.Versions.LastOrDefault();

        var body = new FormExportBodyDto
        {
            Name = source.Name,
            Description = source.Description,
            BrandColor = source.BrandColor,
            ThankYouMessage = source.ThankYouMessage,
            RedirectUrl = source.RedirectUrl,
            MaxSubmissions = source.MaxSubmissions,
            ClosesAt = source.ClosesAt,
            WebhookUrl = source.WebhookUrl,
            OneResponsePerEmail = source.OneResponsePerEmail,
            SendConfirmationEmail = source.SendConfirmationEmail,
            ConfirmationEmailSubject = source.ConfirmationEmailSubject,
            ConfirmationEmailBody = source.ConfirmationEmailBody,
            Fields = version == null
                ? new List<FormExportFieldDto>()
                : version.Fields
                    .OrderBy(f => f.Order)
                    .Select(f => new FormExportFieldDto
                    {
                        Name = f.Name,
                        Label = f.Label,
                        Type = f.Type.ToString(),
                        Order = f.Order,
                        IsRequired = f.IsRequired,
                        IsVisible = f.IsVisible,
                        IsReadOnly = f.IsReadOnly,
                        Placeholder = f.Placeholder,
                        HelpText = f.HelpText,
                        DefaultValue = f.DefaultValue,
                        Validation = f.Validation,
                        Options = f.Options,
                        ShowIfCondition = f.ShowIfCondition,
                    })
                    .ToList(),
        };

        return new FormExportDto
        {
            FormatVersion = 1,
            ExportedAt = DateTime.UtcNow,
            Form = body,
        };
    }

    public async Task<FormDto> ImportFormAsync(FormExportDto payload)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));
        if (payload.FormatVersion != 1)
            throw new InvalidFormExportException($"Unsupported formatVersion {payload.FormatVersion}; this server understands version 1.");
        if (payload.Form == null || string.IsNullOrWhiteSpace(payload.Form.Name))
            throw new InvalidFormExportException("Import payload is missing 'form.name'.");

        ValidateRedirectUrl(payload.Form.RedirectUrl);
        ValidateWebhookUrl(payload.Form.WebhookUrl);

        var now = DateTime.UtcNow;
        var fields = new List<FormBuilder.Models.Entities.FormVersionField>();
        var order = 0;
        foreach (var f in payload.Form.Fields ?? new List<FormExportFieldDto>())
        {
            if (string.IsNullOrWhiteSpace(f.Name) || string.IsNullOrWhiteSpace(f.Label))
                throw new InvalidFormExportException("Every field must carry both 'name' and 'label'.");
            if (!Enum.TryParse<FormBuilder.Models.Entities.FieldType>(f.Type, ignoreCase: true, out var parsedType))
                throw new InvalidFormExportException($"Unknown field type '{f.Type}' on field '{f.Name}'.");
            fields.Add(new FormBuilder.Models.Entities.FormVersionField
            {
                Name = f.Name,
                Label = f.Label,
                Type = parsedType,
                Order = f.Order > 0 ? f.Order : ++order,
                IsRequired = f.IsRequired,
                IsVisible = f.IsVisible,
                IsReadOnly = f.IsReadOnly,
                Placeholder = f.Placeholder,
                HelpText = f.HelpText,
                DefaultValue = f.DefaultValue,
                Validation = f.Validation,
                Options = f.Options,
                ShowIfCondition = f.ShowIfCondition,
            });
        }

        var entity = new FormBuilder.Models.Entities.Form
        {
            Name = payload.Form.Name,
            Description = payload.Form.Description,
            BrandColor = payload.Form.BrandColor,
            ThankYouMessage = payload.Form.ThankYouMessage,
            RedirectUrl = payload.Form.RedirectUrl,
            MaxSubmissions = payload.Form.MaxSubmissions,
            ClosesAt = payload.Form.ClosesAt,
            WebhookUrl = payload.Form.WebhookUrl,
            OneResponsePerEmail = payload.Form.OneResponsePerEmail,
            SendConfirmationEmail = payload.Form.SendConfirmationEmail,
            ConfirmationEmailSubject = payload.Form.ConfirmationEmailSubject,
            ConfirmationEmailBody = payload.Form.ConfirmationEmailBody,
            Slug = SlugGenerator.Generate(),
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now,
            Versions = new List<FormBuilder.Models.Entities.FormVersion>
            {
                new()
                {
                    VersionNumber = 1,
                    Description = "Imported version",
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsPublished = true,
                    IsCurrentVersion = true,
                    Fields = fields,
                },
            },
        };

        var created = await _formRepository.CreateAsync(entity);
        return _mapper.Map<FormDto>(created);
    }

    // Only accept absolute http/https redirects. Blocks javascript:, data:,
    // file:, and relative URLs that would otherwise let the admin construct
    // an open-redirect or client-side XSS trap on the public form page.
    private static void ValidateRedirectUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidRedirectUrlException(url);
        }
    }

    // Same http/https-only rule as ValidateRedirectUrl but throws a distinct
    // exception so the API layer/UI can attribute the error to the right field.
    private static void ValidateWebhookUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidWebhookUrlException(url);
        }
    }

    private async Task<bool> SetFormActiveStateAsync(Guid id, bool isActive)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var form = await _formRepository.GetByIdAsync(id);
        if (form == null)
            throw new FormNotFoundException(id);

        form.IsActive = isActive;
        form.UpdatedAt = DateTime.UtcNow;
        var result = await _formRepository.UpdateAsync(id, form);
        return result != null;
    }
}
