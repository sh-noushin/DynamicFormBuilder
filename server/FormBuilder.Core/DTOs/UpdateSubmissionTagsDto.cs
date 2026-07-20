using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class UpdateSubmissionTagsDto
{
    // Free-text labels. Normalized server-side: trimmed, lower-cased, deduped,
    // empties dropped. Individual tag length capped so a rogue payload can't
    // saturate the 500-char storage cell.
    public List<string> Tags { get; set; } = new List<string>();
}
