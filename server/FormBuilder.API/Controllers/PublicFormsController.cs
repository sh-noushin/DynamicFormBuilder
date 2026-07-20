using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/public/forms")]
[AllowAnonymous]
public class PublicFormsController : ControllerBase
{
    // Clients send the form's shared password via this header when the form
    // has Form.AccessPassword set. The header is optional on GET (returns a
    // stub response with RequiresPassword=true when missing) and required on
    // POST (submit fails with 401 when missing or wrong).
    private const string PasswordHeader = "X-Form-Password";

    private readonly IPublicFormService _publicFormService;
    private readonly IFormSubmissionDraftService _draftService;

    public PublicFormsController(
        IPublicFormService publicFormService,
        IFormSubmissionDraftService draftService)
    {
        _publicFormService = publicFormService;
        _draftService = draftService;
    }

    [HttpGet("{slug}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PublicFormDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<PublicFormDto>> GetForm(string slug)
    {
        var password = Request.Headers.TryGetValue(PasswordHeader, out var v) ? v.ToString() : null;
        var form = await _publicFormService.GetBySlugAsync(slug, password);
        return Ok(form);
    }

    [HttpPost("{slug}/drafts")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDraftDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormSubmissionDraftDto>> SaveDraft(string slug, [FromBody] SaveDraftDto payload)
    {
        var saved = await _draftService.SaveAsync(slug, payload);
        return Ok(saved);
    }

    [HttpGet("{slug}/drafts/{resumeToken:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDraftDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormSubmissionDraftDto>> GetDraft(string slug, Guid resumeToken)
    {
        var draft = await _draftService.GetAsync(slug, resumeToken);
        if (draft == null) return NotFound();
        return Ok(draft);
    }

    [HttpPost("{slug}/submissions")]
    [EnableRateLimiting("public-submit")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 401)]
    [ProducesResponseType(typeof(void), 404)]
    [ProducesResponseType(typeof(void), 429)]
    public async Task<ActionResult<FormSubmissionDto>> Submit(string slug, PublicFormSubmissionDto submission)
    {
        var password = Request.Headers.TryGetValue(PasswordHeader, out var v) ? v.ToString() : null;
        var created = await _publicFormService.SubmitAsync(slug, submission, password);
        return CreatedAtAction(nameof(GetForm), new { slug }, created);
    }
}
