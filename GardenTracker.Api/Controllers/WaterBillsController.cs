using GardenTracker.Api.DTOs.Requests.WaterBills;
using GardenTracker.Api.DTOs.Responses.WaterBills;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/water-bills")]
public class WaterBillsController(IWaterBillService waterBillService, IWaterBillCsvImportService csvImportService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WaterBillResponse>>> GetAll([FromQuery] int? year)
    {
        var bills = await waterBillService.GetAllAsync(CurrentUserId, year);
        return Ok(bills.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WaterBillResponse>> GetById(int id)
    {
        var bill = await waterBillService.GetByIdAsync(id, CurrentUserId);
        return bill == null ? NotFound() : Ok(Map(bill));
    }

    [HttpPost]
    public async Task<ActionResult<WaterBillResponse>> Create(CreateWaterBillRequest request)
    {
        var bill = new WaterBill
        {
            Year = request.Year, Month = request.Month,
            UsageCcf = request.UsageCcf, UsageGallons = request.UsageGallons,
            TotalCost = request.TotalCost, IsGardenActive = request.IsGardenActive,
            Notes = request.Notes
        };
        var created = await waterBillService.CreateAsync(CurrentUserId, bill);
        if (created == null) return Conflict("A water bill for this month already exists.");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateWaterBillRequest request)
    {
        var bill = new WaterBill
        {
            UsageCcf = request.UsageCcf, UsageGallons = request.UsageGallons,
            TotalCost = request.TotalCost, IsGardenActive = request.IsGardenActive,
            Notes = request.Notes
        };
        var updated = await waterBillService.UpdateAsync(id, CurrentUserId, bill);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await waterBillService.DeleteAsync(id, CurrentUserId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        using var stream = file.OpenReadStream();
        var result = await csvImportService.ImportAsync(CurrentUserId, stream);
        return Ok(result);
    }

    private static WaterBillResponse Map(WaterBill w) => new()
    {
        Id = w.Id, Year = w.Year, Month = w.Month,
        UsageCcf = w.UsageCcf, UsageGallons = w.UsageGallons,
        TotalCost = w.TotalCost, IsGardenActive = w.IsGardenActive,
        Notes = w.Notes
    };
}
