using GardenTracker.Api.DTOs.Requests.PlantVarieties;
using GardenTracker.Api.DTOs.Responses.PlantVarieties;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/plant-varieties")]
public class PlantVarietiesController(IPlantVarietyService plantVarietyService, IPlantTypeService plantTypeService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantVarietyResponse>>> GetAll()
    {
        var types = (await plantTypeService.GetAllAsync()).ToDictionary(t => t.Id);
        var varieties = await plantVarietyService.GetAllAsync();
        return Ok(varieties.Select(v => ToResponse(v, types.TryGetValue(v.PlantTypeId, out var t) ? t.Name : string.Empty)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlantVarietyResponse>> GetById(int id)
    {
        var variety = await plantVarietyService.GetByIdAsync(id);
        if (variety == null) return NotFound();
        var type = await plantTypeService.GetByIdAsync(variety.PlantTypeId);
        return Ok(ToResponse(variety, type?.Name ?? string.Empty));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreatePlantVarietyRequest request)
    {
        var updated = await plantVarietyService.UpdateAsync(id, request.Name, request.Notes, request.GrowthHabit, request.DaysToMaturity, request.SpacingInches, request.SunPreference, request.IsPerennial);
        return updated ? NoContent() : NotFound();
    }

    private static PlantVarietyResponse ToResponse(PlantVariety v, string plantTypeName) => new()
    {
        Id = v.Id,
        PlantTypeId = v.PlantTypeId,
        PlantTypeName = plantTypeName,
        Name = v.Name,
        Notes = v.Notes,
        GrowthHabit = v.GrowthHabit?.ToString(),
        DaysToMaturity = v.DaysToMaturity,
        SpacingInches = v.SpacingInches,
        SunPreference = v.SunPreference?.ToString(),
        IsPerennial = v.IsPerennial
    };
}
