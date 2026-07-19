namespace FormBuilder.Core.DTOs;

// Server -> client response after a successful upload. The token is what the
// client stores in FormSubmissionValue.FieldValue for File-typed fields; the
// backend later resolves it to the on-disk path when the submission is viewed.
public class FileUploadResultDto
{
    public string Token { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
