using GardenTracker.Api.DTOs.Requests.PlantTypes;
using GardenTracker.Api.DTOs.Requests.PlantVarieties;
using GardenTracker.Api.DTOs.Responses.PlantTypes;
using GardenTracker.Api.DTOs.Responses.PlantVarieties;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/plant-types")]
public class PlantTypesController(IPlantTypeService plantTypeService, IPlantVarietyService plantVarietyService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantTypeResponse>>> GetAll()
    {
        var types = await plantTypeService.GetAllAsync();
        return Ok(types.Select(t => new PlantTypeResponse { Id = t.Id, Name = t.Name }));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlantTypeResponse>> GetById(int id)
    {
        var type = await plantTypeService.GetByIdAsync(id);
        return type == null ? NotFound() : Ok(new PlantTypeResponse { Id = type.Id, Name = type.Name });
    }

    [HttpPost]
    public async Task<ActionResult<PlantTypeResponse>> Create(CreatePlantTypeRequest request)
    {
        var type = await plantTypeService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, new PlantTypeResponse { Id = type.Id, Name = type.Name });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreatePlantTypeRequest request)
    {
        var updated = await plantTypeService.UpdateAsync(id, request.Name);
        return updated ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/varieties")]
    public async Task<ActionResult<IEnumerable<PlantVarietyResponse>>> GetVarieties(int id)
    {
        var type = await plantTypeService.GetByIdAsync(id);
        if (type == null) return NotFound();
        var varieties = await plantVarietyService.GetByPlantTypeAsync(id);
        return Ok(varieties.Select(v => new PlantVarietyResponse
        {
            Id = v.Id, PlantTypeId = v.PlantTypeId, PlantTypeName = type.Name, Name = v.Name, Notes = v.Notes
        }));
    }

    [HttpPost("{id:int}/varieties")]
    public async Task<ActionResult<PlantVarietyResponse>> CreateVariety(int id, CreatePlantVarietyRequest request)
    {
        var type = await plantTypeService.GetByIdAsync(id);
        if (type == null) return NotFound();
        var variety = await plantVarietyService.CreateAsync(id, request.Name, request.Notes);
        return CreatedAtAction(nameof(GetVarieties), new { id }, new PlantVarietyResponse
        {
            Id = variety.Id, PlantTypeId = variety.PlantTypeId, PlantTypeName = type.Name, Name = variety.Name, Notes = variety.Notes
        });
    }
}
