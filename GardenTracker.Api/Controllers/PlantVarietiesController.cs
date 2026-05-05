using GardenTracker.Api.DTOs.Requests.PlantVarieties;
using GardenTracker.Api.DTOs.Responses.PlantVarieties;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/plant-varieties")]
public class PlantVarietiesController(IPlantVarietyService plantVarietyService, IPlantTypeService plantTypeService) : ApiControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlantVarietyResponse>> GetById(int id)
    {
        var variety = await plantVarietyService.GetByIdAsync(id);
        if (variety == null) return NotFound();
        var type = await plantTypeService.GetByIdAsync(variety.PlantTypeId);
        return Ok(new PlantVarietyResponse
        {
            Id = variety.Id, PlantTypeId = variety.PlantTypeId,
            PlantTypeName = type?.Name ?? string.Empty, Name = variety.Name, Notes = variety.Notes
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreatePlantVarietyRequest request)
    {
        var updated = await plantVarietyService.UpdateAsync(id, request.Name, request.Notes);
        return updated ? NoContent() : NotFound();
    }
}
