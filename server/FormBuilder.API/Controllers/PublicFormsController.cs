using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/public/forms")]
[AllowAnonymous]
public class PublicFormsController : ControllerBase
{
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
        var form = await _publicFormService.GetBySlugAsync(slug);
        return Ok(form);
    }

    [HttpPost("{slug}/submissions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormSubmissionDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormSubmissionDto>> Submit(string slug, PublicFormSubmissionDto submission)
    {
        var created = await _publicFormService.SubmitAsync(slug, submission);
        return CreatedAtAction(nameof(GetForm), new { slug }, created);
    }
}
