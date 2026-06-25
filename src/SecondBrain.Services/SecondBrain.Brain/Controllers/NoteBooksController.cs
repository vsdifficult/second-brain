using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;

namespace SecondBrain.Services.BrainService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NoteBooksController : ControllerBase
{
    private readonly INoteBookService _noteBookService;

    public NoteBooksController(INoteBookService noteBookService)
    {
        _noteBookService = noteBookService;
    }

    /// <summary>GET /api/notebooks/{id}</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try
        {
            var notebook = await _noteBookService.GetNoteBookAsync(id, ct);
            return Ok(notebook);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }

    /// <summary>POST /api/notebooks</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NoteBookCreateRequestDto dto, CancellationToken ct)
    {
        try
        {
            var id = await _noteBookService.CreateNoteBookAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }
        catch (ArgumentException ex) { return BadRequest(new { ex.Message }); }
    }

    /// <summary>PUT /api/notebooks/{id}</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] NoteBookUpdateRequestDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _noteBookService.UpdateNoteBookAsync(id, dto.Name, ct);
            return Ok(new { result });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
        catch (NotImplementedException) { return StatusCode(501, new { Message = "Not implemented yet" }); }
    }

    /// <summary>DELETE /api/notebooks/{id}</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _noteBookService.DeleteNoteBookAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }

    /// <summary>POST /api/notebooks/notes/{noteId} — привязать заметку к блокноту</summary>
    [HttpPost("notes/{noteId:guid}")]
    public async Task<IActionResult> AddNote(Guid noteId, CancellationToken ct)
    {
        try
        {
            var result = await _noteBookService.AddNoteAsync(noteId, ct);
            return Ok(new { result });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }
}
