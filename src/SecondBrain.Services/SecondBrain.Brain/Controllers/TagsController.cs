using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBrain.Services.BrainService.Models;
using SecondBrain.Services.BrainService.Services.Interfaces;

namespace SecondBrain.Services.BrainService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>GET /api/tags/{id}</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try
        {
            var tag = await _tagService.GetTagAsync(id, ct);
            return Ok(tag);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }

    /// <summary>GET /api/tags/note/{noteId} — все теги заметки</summary>
    [HttpGet("note/{noteId:guid}")]
    public async Task<IActionResult> GetByNote(Guid noteId, CancellationToken ct)
    {
        var tags = await _tagService.GetTagsByNoteAsync(noteId, ct);
        return Ok(tags);
    }

    /// <summary>POST /api/tags</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TagCreate dto, CancellationToken ct)
    {
        try
        {
            var id = await _tagService.CreateTagAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }
        catch (ArgumentException ex) { return BadRequest(new { ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { ex.Message }); }
    }

    /// <summary>POST /api/tags/apply — применить тег к заметке</summary>
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyTagToNote dto, CancellationToken ct)
    {
        try
        {
            var result = await _tagService.ApplyTagToNoteAsync(dto, ct);
            return Ok(new { result });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { ex.Message }); }
    }

    /// <summary>DELETE /api/tags/apply — снять тег с заметки</summary>
    [HttpDelete("apply")]
    public async Task<IActionResult> RemoveFromNote([FromBody] ApplyTagToNote dto, CancellationToken ct)
    {
        try
        {
            await _tagService.RemoveTagFromNoteAsync(dto, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }

    /// <summary>DELETE /api/tags/{id}</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _tagService.DeleteTagAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { ex.Message }); }
    }
}
