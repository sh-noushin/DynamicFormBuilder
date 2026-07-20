using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class UpdateSubmissionNotesDto
{
    [StringLength(4000)]
    public string? AdminNotes { get; set; }
}
