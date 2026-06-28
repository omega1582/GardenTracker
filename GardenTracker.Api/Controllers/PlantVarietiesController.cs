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
        var updated = await plantVarietyService.UpdateAsync(id, request.Name, request.Notes, request.GrowthHabit, request.DaysToMaturity, request.SpacingInches, request.SunPreference, request.IsPerennial, request.ImageUrl);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("upload-image")]
    public async Task<ActionResult<object>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { error = "Invalid image format. Allowed formats are JPG, JPEG, PNG, GIF, WEBP." });

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/{uniqueFileName}";
        return Ok(new { Url = relativeUrl });
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
        IsPerennial = v.IsPerennial,
        ImageUrl = v.ImageUrl
    };
}
