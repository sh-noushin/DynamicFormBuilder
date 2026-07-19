namespace FormBuilder.Core.Options;

public sealed class FileUploadOptions
{
    public const string SectionName = "FileUploads";

    // Absolute or relative directory where uploaded files are stored.
    public string StoragePath { get; set; } = "uploads";

    // Maximum file size in bytes. Default: 10 MB.
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    // Allowed MIME type prefixes (e.g. "image/", "application/pdf"). Empty = all allowed.
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
}
