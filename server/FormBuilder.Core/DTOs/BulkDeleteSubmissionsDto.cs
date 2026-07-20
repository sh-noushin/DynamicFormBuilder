using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

public class BulkDeleteSubmissionsDto
{
    [Required, MinLength(1)]
    public List<Guid> Ids { get; set; } = new List<Guid>();
}

public class BulkDeleteResultDto
{
    public int Deleted { get; set; }
}
