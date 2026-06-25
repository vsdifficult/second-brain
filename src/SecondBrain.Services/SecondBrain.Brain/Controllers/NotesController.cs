using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;

namespace SecondBrain.Services.BrainService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    /// <summary>GET /api/notes/{id}</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try
        {
            var note = await _noteService.GetNoteAsync(id, ct);
            return Ok(note);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }

    /// <summary>POST /api/notes</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NoteCreateRequestDto dto, CancellationToken ct)
    {
        try
        {
            var id = await _noteService.CreateNoteAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }
        catch (ArgumentException ex) { return BadRequest(new { ex.Message }); }
    }

    /// <summary>PUT /api/notes/{id}</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] NoteUpdateRequestDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _noteService.UpdateNoteAsync(id, dto.Body, ct);
            return Ok(new { result });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ex.Message }); }
    }

    /// <summary>DELETE /api/notes/{id}</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _noteService.DeleteNoteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }
}
