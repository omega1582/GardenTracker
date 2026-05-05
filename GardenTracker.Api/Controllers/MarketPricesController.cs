using GardenTracker.Api.DTOs.Requests.MarketPrices;
using GardenTracker.Api.DTOs.Responses.MarketPrices;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/gardens/{gardenId:int}/seasons/{year:int}/market-prices")]
public class MarketPricesController(IMarketPriceService marketPriceService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarketPriceResponse>>> GetAll(int gardenId, int year)
    {
        var prices = await marketPriceService.GetBySeasonAsync(gardenId, year, CurrentUserId);
        return Ok(prices.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MarketPriceResponse>> GetById(int gardenId, int year, int id)
    {
        var price = await marketPriceService.GetByIdAsync(id);
        return price == null ? NotFound() : Ok(Map(price));
    }

    [HttpPost]
    public async Task<ActionResult<MarketPriceResponse>> Create(int gardenId, int year, CreateMarketPriceRequest request)
    {
        var price = new MarketPrice
        {
            PlantTypeId = request.PlantTypeId, PlantVarietyId = request.PlantVarietyId,
            PricePerUnit = request.PricePerUnit, Unit = request.Unit, RecordedDate = request.RecordedDate
        };
        var created = await marketPriceService.CreateAsync(gardenId, year, CurrentUserId, price);
        return CreatedAtAction(nameof(GetById), new { gardenId, year, id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int gardenId, int year, int id, CreateMarketPriceRequest request)
    {
        var updated = await marketPriceService.UpdateAsync(id, CurrentUserId, request.PricePerUnit, request.Unit, request.RecordedDate);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int gardenId, int year, int id)
    {
        var deleted = await marketPriceService.DeleteAsync(id, CurrentUserId);
        return deleted ? NoContent() : NotFound();
    }

    private static MarketPriceResponse Map(MarketPrice m) => new()
    {
        Id = m.Id, SeasonId = m.SeasonId, PlantTypeId = m.PlantTypeId,
        PlantTypeName = m.PlantType?.Name ?? string.Empty,
        PlantVarietyId = m.PlantVarietyId, PlantVarietyName = m.PlantVariety?.Name,
        PricePerUnit = m.PricePerUnit, Unit = m.Unit, RecordedDate = m.RecordedDate
    };
}
