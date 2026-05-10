using GardenTracker.Api.DTOs.Requests.Plantings;
using GardenTracker.Api.DTOs.Responses.Plantings;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/gardens/{gardenId:int}/seasons/{year:int}/plantings")]
public class PlantingsController(IPlantingService plantingService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantingResponse>>> GetAll(
        int gardenId, int year, [FromQuery] int? bedId, [FromQuery] int? plantTypeId)
    {
        var plantings = await plantingService.GetBySeasonAsync(gardenId, year, CurrentUserId, bedId, plantTypeId);
        return Ok(plantings.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlantingResponse>> GetById(int gardenId, int year, int id)
    {
        var planting = await plantingService.GetByIdAsync(id, CurrentUserId);
        return planting == null ? NotFound() : Ok(Map(planting));
    }

    [HttpPost]
    public async Task<ActionResult<PlantingResponse>> Create(int gardenId, int year, CreatePlantingRequest request)
    {
        var planting = new BedPlanting
        {
            BedId = request.BedId, PlantVarietyId = request.PlantVarietyId,
            SupplierId = request.SupplierId, StartMethod = request.StartMethod,
            Quantity = request.Quantity, TotalCost = request.TotalCost,
            SourceHarvestId = request.SourceHarvestId, Notes = request.Notes,
            InventoryItemId = request.InventoryItemId
        };
        var created = await plantingService.CreateAsync(gardenId, year, CurrentUserId, planting, request.QuantityUsedFromInventory);
        return CreatedAtAction(nameof(GetById), new { gardenId, year, id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int gardenId, int year, int id, UpdatePlantingRequest request)
    {
        var updated = await plantingService.UpdateAsync(id, CurrentUserId, request.SupplierId,
            request.StartMethod, request.Quantity, request.TotalCost, request.SourceHarvestId, request.Notes,
            request.InventoryItemId, request.QuantityUsedFromInventory);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int gardenId, int year, int id)
    {
        var deleted = await plantingService.DeleteAsync(id, CurrentUserId);
        return deleted ? NoContent() : NotFound();
    }

    private static PlantingResponse Map(BedPlanting p) => new()
    {
        Id = p.Id, BedId = p.BedId, BedName = p.BedName ?? string.Empty,
        SeasonId = p.SeasonId, PlantVarietyId = p.PlantVarietyId,
        PlantVarietyName = p.PlantVarietyName ?? string.Empty,
        PlantTypeName = p.PlantTypeName ?? string.Empty,
        SupplierId = p.SupplierId, SupplierName = p.SupplierName,
        StartMethod = p.StartMethod, Quantity = p.Quantity, TotalCost = p.TotalCost,
        SourceHarvestId = p.SourceHarvestId, Notes = p.Notes,
        InventoryItemId = p.InventoryItemId, QuantityUsedFromInventory = p.QuantityUsedFromInventory
    };
}
