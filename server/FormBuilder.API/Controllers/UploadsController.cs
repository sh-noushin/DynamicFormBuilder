using FormBuilder.Core.DTOs;
using FormBuilder.Core.Options;
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

    public UploadsController(
        IOptions<FileUploadOptions> options,
        IWebHostEnvironment env,
        ILogger<UploadsController> logger)
    {
        _options = options.Value;
        _env = env;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FileUploadResultDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 413)]
    [RequestSizeLimit(64 * 1024 * 1024)]
    public async Task<ActionResult<FileUploadResultDto>> Upload(IFormFile file)
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

    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        // Truncate very long names to keep filesystem happy.
        return cleaned.Length > 120 ? cleaned[..120] : cleaned;
    }
}
