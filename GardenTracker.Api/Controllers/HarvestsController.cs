using GardenTracker.Api.DTOs.Requests.Harvests;
using GardenTracker.Api.DTOs.Responses.Harvests;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/gardens/{gardenId:int}/seasons/{year:int}/harvests")]
public class HarvestsController(IHarvestService harvestService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HarvestResponse>>> GetAll(
        int gardenId, int year, [FromQuery] int? bedId, [FromQuery] int? plantVarietyId)
    {
        var harvests = await harvestService.GetBySeasonAsync(gardenId, year, CurrentUserId, bedId, plantVarietyId);
        return Ok(harvests.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HarvestResponse>> GetById(int gardenId, int year, int id)
    {
        var harvest = await harvestService.GetByIdAsync(id, CurrentUserId);
        return harvest == null ? NotFound() : Ok(Map(harvest));
    }

    [HttpPost]
    public async Task<ActionResult<HarvestResponse>> Create(int gardenId, int year, CreateHarvestRequest request)
    {
        var harvest = new Harvest
        {
            BedId = request.BedId, PlantVarietyId = request.PlantVarietyId,
            Quantity = request.Quantity, Unit = request.Unit,
            HarvestDate = request.HarvestDate, Notes = request.Notes
        };
        var created = await harvestService.CreateAsync(gardenId, year, CurrentUserId, harvest);
        return CreatedAtAction(nameof(GetById), new { gardenId, year, id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int gardenId, int year, int id, UpdateHarvestRequest request)
    {
        var updated = await harvestService.UpdateAsync(id, CurrentUserId, request.Quantity, request.Unit, request.HarvestDate, request.Notes);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int gardenId, int year, int id)
    {
        var deleted = await harvestService.DeleteAsync(id, CurrentUserId);
        return deleted ? NoContent() : NotFound();
    }

    private static HarvestResponse Map(Harvest h) => new()
    {
        Id = h.Id, BedId = h.BedId, BedName = h.BedName ?? string.Empty,
        SeasonId = h.SeasonId, PlantVarietyId = h.PlantVarietyId,
        PlantVarietyName = h.PlantVarietyName ?? string.Empty,
        PlantTypeName = h.PlantTypeName ?? string.Empty,
        Quantity = h.Quantity, Unit = h.Unit, HarvestDate = h.HarvestDate, Notes = h.Notes
    };
}
