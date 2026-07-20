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

    public PublicFormsController(IPublicFormService publicFormService)
    {
        _publicFormService = publicFormService;
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
