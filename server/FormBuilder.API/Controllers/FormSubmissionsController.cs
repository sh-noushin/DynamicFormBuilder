using System.Globalization;
using System.Text;
using FormBuilder.Core.Common;
using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormSubmissionsController : ControllerBase
{
    private readonly IFormSubmissionService _submissionService;
    private readonly IFormVersionService _formVersionService;

    public FormSubmissionsController(
        IFormSubmissionService submissionService,
        IFormVersionService formVersionService)
    {
        _submissionService = submissionService;
        _formVersionService = formVersionService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<ActionResult<FormSubmissionDto>> CreateSubmission(CreateFormSubmissionDto createDto)
    {
        try
        {
            var formVersion = await _formVersionService.GetVersionByIdAsync(createDto.FormVersionId);

            if (!formVersion.IsPublished)
            {
                return BadRequest("Cannot submit to an unpublished form version");
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var values = (createDto.FieldValues ?? new Dictionary<string, string?>())
                .Select(kv => new FormSubmissionValue { FieldName = kv.Key, FieldValue = kv.Value })
                .ToList();

            var createdSubmission = await _submissionService.CreateSubmissionAsync(createDto);
            return CreatedAtAction(nameof(GetSubmission), new { id = createdSubmission.Id }, createdSubmission);
        }
        catch (FormBuilder.Models.Exceptions.FormSubmissionValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.FieldErrors });
        }
        catch (FormVersionNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return Problem(detail: message, statusCode: 500, title: "Failed to create submission");
        }

    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormSubmissionDto>> GetSubmission(Guid id)
    {
        var submission = await _submissionService.GetSubmissionByIdAsync(id);
        if (submission == null)
        {
            return NotFound();
        }

        return Ok(submission);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormSubmissionDto>> UpdateSubmission(Guid id, UpdateFormSubmissionDto updateDto)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid submission identifier");
        }

        updateDto ??= new UpdateFormSubmissionDto();

        try
        {
            var updated = await _submissionService.UpdateSubmissionAsync(
                id,
                updateDto);

            return Ok(updated);
        }
        catch (FormSubmissionNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return Problem(detail: message, statusCode: 500, title: "Failed to update submission");
        }
    }

    [HttpGet("form-version/{formVersionId}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<FormSubmissionDto>), 200)]
    public async Task<ActionResult<IEnumerable<FormSubmissionDto>>> GetSubmissionsByFormVersion(Guid formVersionId)
    {
        var submissions = await _submissionService.GetSubmissionsByFormVersionIdAsync(formVersionId);
        return Ok(submissions);
    }

    [HttpGet("form/{formId}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<FormSubmissionDto>), 200)]
    public async Task<ActionResult<IEnumerable<FormSubmissionDto>>> GetSubmissionsByForm(Guid formId)
    {
        var submissions = await _submissionService.GetSubmissionsByFormIdAsync(formId);
        return Ok(submissions);
    }

    [HttpGet("form/{formId}/export.csv")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("text/csv")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> ExportFormSubmissions(Guid formId)
    {
        var currentVersion = await _formVersionService.GetCurrentVersionAsync(formId);
        var submissions = await _submissionService.GetSubmissionsByFormIdAsync(formId);

        var fieldColumns = currentVersion.Fields
            .OrderBy(f => f.Order)
            .Select(f => f.Name)
            .ToList();

        var headers = new List<string> { "SubmittedAt", "SubmitterName", "SubmitterEmail" };
        headers.AddRange(fieldColumns);

        var rows = submissions.Select(submission =>
        {
            var byName = submission.Values.ToDictionary(v => v.FieldName, v => v.FieldValue, StringComparer.OrdinalIgnoreCase);
            var row = new List<string?>
            {
                submission.SubmittedAt.ToString("O", CultureInfo.InvariantCulture),
                submission.SubmitterName,
                submission.SubmitterEmail
            };
            row.AddRange(fieldColumns.Select(col => byName.TryGetValue(col, out var v) ? v : null));
            return row;
        });

        var csv = CsvWriter.Write(headers, rows);
        var fileName = $"submissions-{formId}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv), "text/csv", fileName);
    }

    [HttpGet("form-version/{formVersionId}/count")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<ActionResult<int>> GetSubmissionCountByFormVersion(Guid formVersionId)
    {
        var count = await _submissionService.GetSubmissionCountByFormVersionIdAsync(formVersionId);
        return Ok(count);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> DeleteSubmission(Guid id)
    {
        var result = await _submissionService.DeleteSubmissionAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id}/notes")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormSubmissionDto>> UpdateAdminNotes(Guid id, [FromBody] UpdateSubmissionNotesDto payload)
    {
        var updated = await _submissionService.UpdateAdminNotesAsync(id, payload.AdminNotes);
        return Ok(updated);
    }

    [HttpPatch("{id}/tags")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormSubmissionDto>> UpdateTags(Guid id, [FromBody] UpdateSubmissionTagsDto payload)
    {
        var updated = await _submissionService.UpdateTagsAsync(id, payload.Tags ?? new List<string>());
        return Ok(updated);
    }

    [HttpPost("form/{formId}/bulk-delete")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BulkDeleteResultDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<ActionResult<BulkDeleteResultDto>> BulkDeleteSubmissions(Guid formId, [FromBody] BulkDeleteSubmissionsDto payload)
    {
        var deleted = await _submissionService.BulkDeleteSubmissionsAsync(formId, payload.Ids);
        return Ok(new BulkDeleteResultDto { Deleted = deleted });
    }
}

   
