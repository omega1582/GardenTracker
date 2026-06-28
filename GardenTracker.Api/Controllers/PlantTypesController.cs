using GardenTracker.Api.DTOs.Requests.PlantTypes;
using GardenTracker.Api.DTOs.Requests.PlantVarieties;
using GardenTracker.Api.DTOs.Responses.PlantTypes;
using GardenTracker.Api.DTOs.Responses.PlantVarieties;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/plant-types")]
public class PlantTypesController(IPlantTypeService plantTypeService, IPlantVarietyService plantVarietyService, IPlantCatalogCsvImportService csvImportService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantTypeResponse>>> GetAll()
    {
        var types = await plantTypeService.GetAllAsync();
        return Ok(types.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlantTypeResponse>> GetById(int id)
    {
        var type = await plantTypeService.GetByIdAsync(id);
        return type == null ? NotFound() : Ok(ToResponse(type));
    }

    [HttpPost]
    public async Task<ActionResult<PlantTypeResponse>> Create(CreatePlantTypeRequest request)
    {
        var type = await plantTypeService.CreateAsync(request.Name, request.Category, request.GrowthHabit, request.DaysToMaturity, request.SpacingInches, request.SunPreference, request.IsPerennial);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, ToResponse(type));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreatePlantTypeRequest request)
    {
        var updated = await plantTypeService.UpdateAsync(id, request.Name, request.Category, request.GrowthHabit, request.DaysToMaturity, request.SpacingInches, request.SunPreference, request.IsPerennial);
        return updated ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/varieties")]
    public async Task<ActionResult<IEnumerable<PlantVarietyResponse>>> GetVarieties(int id)
    {
        var type = await plantTypeService.GetByIdAsync(id);
        if (type == null) return NotFound();
        var varieties = await plantVarietyService.GetByPlantTypeAsync(id);
        return Ok(varieties.Select(v => ToVarietyResponse(v, type.Name)));
    }

    [HttpPost("{id:int}/varieties")]
    public async Task<ActionResult<PlantVarietyResponse>> CreateVariety(int id, CreatePlantVarietyRequest request)
    {
        var type = await plantTypeService.GetByIdAsync(id);
        if (type == null) return NotFound();
        var variety = await plantVarietyService.CreateAsync(id, request.Name, request.Notes, request.GrowthHabit, request.DaysToMaturity, request.SpacingInches, request.SunPreference, request.IsPerennial, request.ImageUrl);
        return CreatedAtAction(nameof(GetVarieties), new { id }, ToVarietyResponse(variety, type.Name));
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        using var stream = file.OpenReadStream();
        var result = await csvImportService.ImportAsync(stream);
        return Ok(result);
    }

    private static PlantTypeResponse ToResponse(PlantType t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Category = t.Category.ToString(),
        GrowthHabit = t.GrowthHabit?.ToString(),
        DaysToMaturity = t.DaysToMaturity,
        SpacingInches = t.SpacingInches,
        SunPreference = t.SunPreference?.ToString(),
        IsPerennial = t.IsPerennial
    };

    private static PlantVarietyResponse ToVarietyResponse(PlantVariety v, string plantTypeName) => new()
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
        IsPerennial = v.IsPerennial,
        ImageUrl = v.ImageUrl
    };
}
