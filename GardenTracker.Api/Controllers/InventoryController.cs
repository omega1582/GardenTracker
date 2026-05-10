using GardenTracker.Api.DTOs.Requests.Inventory;
using GardenTracker.Api.DTOs.Responses.Inventory;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

public class InventoryController(IInventoryService inventoryService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemResponse>>> GetAll()
    {
        var items = await inventoryService.GetByUserAsync(CurrentUserId);
        return Ok(items.Select(Map));
    }

    [HttpGet("variety/{plantVarietyId:int}")]
    public async Task<ActionResult<IEnumerable<InventoryItemResponse>>> GetByVariety(int plantVarietyId)
    {
        var items = await inventoryService.GetByVarietyAsync(plantVarietyId, CurrentUserId);
        return Ok(items.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InventoryItemResponse>> GetById(int id)
    {
        var item = await inventoryService.GetByIdAsync(id, CurrentUserId);
        return item == null ? NotFound() : Ok(Map(item));
    }

    [HttpPost]
    public async Task<ActionResult<InventoryItemResponse>> Create(CreateInventoryItemRequest request)
    {
        var item = new InventoryItem
        {
            PlantVarietyId = request.PlantVarietyId,
            SupplierId = request.SupplierId,
            Type = request.Type,
            QuantityPurchased = request.QuantityPurchased,
            TotalCost = request.TotalCost,
            PurchaseDate = request.PurchaseDate,
            Notes = request.Notes
        };
        var created = await inventoryService.CreateAsync(CurrentUserId, item);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateInventoryItemRequest request)
    {
        var updated = await inventoryService.UpdateAsync(id, CurrentUserId, request.SupplierId,
            request.QuantityPurchased, request.TotalCost, request.PurchaseDate, request.Notes);
        return updated ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/adjust")]
    public async Task<IActionResult> AdjustRemaining(int id, AdjustInventoryRequest request)
    {
        var adjusted = await inventoryService.AdjustRemainingAsync(id, CurrentUserId, request.NewRemaining);
        return adjusted ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await inventoryService.DeleteAsync(id, CurrentUserId);
        return deleted ? NoContent() : NotFound();
    }

    private static InventoryItemResponse Map(InventoryItem i) => new()
    {
        Id = i.Id,
        PlantVarietyId = i.PlantVarietyId,
        PlantVarietyName = i.PlantVarietyName ?? string.Empty,
        PlantTypeName = i.PlantTypeName ?? string.Empty,
        SupplierId = i.SupplierId,
        SupplierName = i.SupplierName,
        Type = i.Type,
        QuantityPurchased = i.QuantityPurchased,
        QuantityRemaining = i.QuantityRemaining,
        TotalCost = i.TotalCost,
        PurchaseDate = i.PurchaseDate,
        Notes = i.Notes
    };
}
