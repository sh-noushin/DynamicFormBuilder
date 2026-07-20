using System.Text.Json;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Options;
using FormBuilder.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/uploads")]
[AllowAnonymous]
public class UploadsController : ControllerBase
{
    private readonly FileUploadOptions _options;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadsController> _logger;
    private readonly IFormRepository _formRepository;

    public UploadsController(
        IOptions<FileUploadOptions> options,
        IWebHostEnvironment env,
        ILogger<UploadsController> logger,
        IFormRepository formRepository)
    {
        _options = options.Value;
        _env = env;
        _logger = logger;
        _formRepository = formRepository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FileUploadResultDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 413)]
    [RequestSizeLimit(64 * 1024 * 1024)]
    public async Task<ActionResult<FileUploadResultDto>> Upload(
        IFormFile file,
        [FromQuery] string? slug = null,
        [FromQuery] string? fieldName = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file was provided." });

        if (file.Length > _options.MaxFileSizeBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new { message = $"File exceeds maximum size of {_options.MaxFileSizeBytes} bytes." });

        if (_options.AllowedContentTypes.Length > 0)
        {
            var contentType = file.ContentType ?? string.Empty;
            var allowed = _options.AllowedContentTypes.Any(prefix =>
                contentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            if (!allowed)
                return BadRequest(new { message = $"Content type '{contentType}' is not allowed." });
        }

        // Per-field constraints layer on top of the system-wide limits. When the
        // client tells us which form+field this upload belongs to, we can reject
        // based on the field's own size and extension allowlist encoded in the
        // Validation JSON blob. Missing / bad references fall through silently
        // so the system-wide check is still the floor.
        if (!string.IsNullOrWhiteSpace(slug) && !string.IsNullOrWhiteSpace(fieldName))
        {
            var perFieldError = await ValidateAgainstFieldAsync(slug, fieldName, file);
            if (perFieldError is not null)
                return BadRequest(new { message = perFieldError });
        }

        var storageDir = Path.IsPathRooted(_options.StoragePath)
            ? _options.StoragePath
            : Path.Combine(_env.ContentRootPath, _options.StoragePath);
        Directory.CreateDirectory(storageDir);

        var token = Guid.NewGuid().ToString("N");
        var safeName = SanitizeFileName(file.FileName);
        var storedFileName = $"{token}__{safeName}";
        var storedPath = Path.Combine(storageDir, storedFileName);

        await using (var stream = System.IO.File.Create(storedPath))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("Stored upload {Token} ({Original}) [{Size} bytes]", token, safeName, file.Length);

        var result = new FileUploadResultDto
        {
            Token = storedFileName,
            OriginalFileName = safeName,
            SizeBytes = file.Length,
            ContentType = file.ContentType ?? "application/octet-stream"
        };
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{token}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(void), 404)]
    public IActionResult Download(string token)
    {
        // Token was returned as the stored filename; reject anything with path separators.
        if (string.IsNullOrWhiteSpace(token) || token.Contains('/') || token.Contains('\\') || token.Contains(".."))
            return NotFound();

        var storageDir = Path.IsPathRooted(_options.StoragePath)
            ? _options.StoragePath
            : Path.Combine(_env.ContentRootPath, _options.StoragePath);
        var path = Path.Combine(storageDir, token);
        if (!System.IO.File.Exists(path)) return NotFound();

        var originalName = token.Contains("__") ? token[(token.IndexOf("__") + 2)..] : token;
        var contentType = "application/octet-stream";
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (provider.TryGetContentType(originalName, out var resolved))
            contentType = resolved;

        var stream = System.IO.File.OpenRead(path);
        return File(stream, contentType, originalName);
    }

    // Returns a user-facing error string if the file violates a per-field
    // constraint, or null when it is acceptable (or the field lookup fails,
    // which we treat as "no per-field constraint applies").
    private async Task<string?> ValidateAgainstFieldAsync(string slug, string fieldName, IFormFile file)
    {
        var form = await _formRepository.GetBySlugAsync(slug);
        if (form is null) return null;

        var currentVersion = form.Versions.FirstOrDefault(v => v.IsCurrentVersion && v.IsPublished);
        var field = currentVersion?.Fields.FirstOrDefault(f => f.Name == fieldName);
        if (field is null || string.IsNullOrWhiteSpace(field.Validation)) return null;

        try
        {
            using var doc = JsonDocument.Parse(field.Validation);
            var root = doc.RootElement;

            if (root.TryGetProperty("maxFileSizeMb", out var maxProp) &&
                maxProp.TryGetInt32(out var maxMb) && maxMb > 0)
            {
                var maxBytes = (long)maxMb * 1024L * 1024L;
                if (file.Length > maxBytes)
                    return $"File exceeds the maximum size of {maxMb} MB for this field.";
            }

            if (root.TryGetProperty("allowedFileExtensions", out var extProp) &&
                extProp.ValueKind == JsonValueKind.Array)
            {
                var allowed = extProp.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => (e.GetString() ?? string.Empty).TrimStart('.').Trim().ToLowerInvariant())
                    .Where(s => s.Length > 0)
                    .ToArray();
                if (allowed.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
                    if (!allowed.Contains(ext))
                        return $"File type '.{ext}' is not allowed. Allowed: {string.Join(", ", allowed.Select(a => "." + a))}.";
                }
            }
        }
        catch (JsonException)
        {
            // A malformed Validation JSON should not prevent the upload -
            // the admin will see the config error via the field validator
            // when the submission comes in.
        }
        return null;
    }

    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        // Truncate very long names to keep filesystem happy.
        return cleaned.Length > 120 ? cleaned[..120] : cleaned;
    }
}
