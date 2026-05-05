using GardenTracker.Api.DTOs.Requests.Gardens;
using GardenTracker.Api.DTOs.Responses.Gardens;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/gardens")]
public class GardensController(IGardenService gardenService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GardenResponse>>> GetAll()
    {
        var gardens = await gardenService.GetByUserAsync(CurrentUserId);
        return Ok(gardens.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GardenResponse>> GetById(int id)
    {
        var garden = await gardenService.GetByIdAsync(id, CurrentUserId);
        return garden == null ? NotFound() : Ok(Map(garden));
    }

    [HttpPost]
    public async Task<ActionResult<GardenResponse>> Create(CreateGardenRequest request)
    {
        var garden = await gardenService.CreateAsync(CurrentUserId, request.Name, request.Location, request.Notes);
        return CreatedAtAction(nameof(GetById), new { id = garden.Id }, Map(garden));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateGardenRequest request)
    {
        var updated = await gardenService.UpdateAsync(id, CurrentUserId, request.Name, request.Location, request.Notes);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await gardenService.DeleteAsync(id, CurrentUserId);
        return deleted ? NoContent() : NotFound();
    }

    private static GardenResponse Map(Core.Entities.Garden g) => new()
    {
        Id = g.Id, Name = g.Name, Location = g.Location, Notes = g.Notes, CreatedAt = g.CreatedAt
    };
}
