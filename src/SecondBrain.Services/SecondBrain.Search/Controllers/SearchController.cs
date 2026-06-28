using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecondBrain.Services.SearchService.Services.Interfaces; 

namespace SecondBrain.Services.SearchService.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService) => _searchService = searchService;

    // GET /api/search?q=zettelkasten
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(ownerIdClaim, out var ownerId))
            return Unauthorized();

        var results = await _searchService.SearchAsync(q, ownerId, ct);
        return Ok(results);
    }
}