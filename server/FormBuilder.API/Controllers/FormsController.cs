using FormBuilder.Core.DTOs;
using FormBuilder.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FormBuilder.Core.Interfaces;
using AutoMapper;

namespace FormBuilder.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormsController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IFormVersionService _formVersionService;
        private readonly IMapper _mapper;

        public FormsController(IFormService formService, IFormVersionService formVersionService, IMapper mapper)
        {
            _formService = formService;
            _formVersionService = formVersionService;
            _mapper = mapper;
        }



        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<FormDto>), 200)]
        public async Task<IActionResult> GetAllForms()
        {
            var forms = await _formService.GetAllFormsAsync();
            return Ok(forms);
        }


        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(FormDto), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> GetForm(Guid id)
        {
            var form = await _formService.GetFormByIdAsync(id);
            return form == null ? NotFound() : Ok(form);
        }


        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(FormDto), 201)]
        public async Task<IActionResult> CreateForm(CreateFormDto createFormDto)
        {
            var createdForm = await _formService.CreateFormAsync(createFormDto);
            return CreatedAtAction(nameof(GetForm), new { id = createdForm.Id }, createdForm);
        }


        [HttpPut("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(FormDto), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> UpdateForm(Guid id, UpdateFormDto updateFormDto)
        {
            var updatedForm = await _formService.UpdateFormAsync(id, updateFormDto);
            if (updatedForm == null)
            {
                return NotFound();
            }
            return Ok(updatedForm);
        }

        // Remove static mapping methods
    }
}
