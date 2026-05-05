using GardenTracker.Api.DTOs.Requests.Seasons;
using GardenTracker.Api.DTOs.Responses.Seasons;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/gardens/{gardenId:int}/seasons")]
public class SeasonsController(ISeasonService seasonService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SeasonResponse>>> GetAll(int gardenId)
    {
        var seasons = await seasonService.GetByGardenAsync(gardenId, CurrentUserId);
        return Ok(seasons.Select(Map));
    }

    [HttpGet("{year:int}")]
    public async Task<ActionResult<SeasonResponse>> GetByYear(int gardenId, int year)
    {
        var season = await seasonService.GetByYearAsync(gardenId, year, CurrentUserId);
        return season == null ? NotFound() : Ok(Map(season));
    }

    [HttpPost]
    public async Task<ActionResult<SeasonResponse>> Create(int gardenId, CreateSeasonRequest request)
    {
        var season = await seasonService.CreateAsync(gardenId, CurrentUserId, request.Year, request.Notes);
        return CreatedAtAction(nameof(GetByYear), new { gardenId, year = season.Year }, Map(season));
    }

    [HttpPut("{year:int}")]
    public async Task<IActionResult> Update(int gardenId, int year, UpdateSeasonRequest request)
    {
        var updated = await seasonService.UpdateAsync(gardenId, year, CurrentUserId, request.Notes);
        return updated ? NoContent() : NotFound();
    }

    private static SeasonResponse Map(Core.Entities.Season s) => new()
    {
        Id = s.Id, GardenId = s.GardenId, Year = s.Year, Notes = s.Notes
    };
}
