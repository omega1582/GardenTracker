using GardenTracker.Api.DTOs.Requests.Beds;
using GardenTracker.Api.DTOs.Responses.Beds;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/gardens/{gardenId:int}/beds")]
public class BedsController(IRaisedBedService bedService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BedResponse>>> GetAll(int gardenId)
    {
        var beds = await bedService.GetByGardenAsync(gardenId, CurrentUserId);
        return Ok(beds.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BedResponse>> GetById(int gardenId, int id)
    {
        var bed = await bedService.GetByIdAsync(id, CurrentUserId);
        return bed == null ? NotFound() : Ok(Map(bed));
    }

    [HttpPost]
    public async Task<ActionResult<BedResponse>> Create(int gardenId, CreateBedRequest request)
    {
        var bed = new RaisedBed
        {
            Name = request.Name, LengthFt = request.LengthFt, WidthFt = request.WidthFt,
            DepthIn = request.DepthIn, Material = request.Material,
            ExpectedLifespanYears = request.ExpectedLifespanYears,
            InstalledDate = request.InstalledDate, Notes = request.Notes
        };
        var created = await bedService.CreateAsync(gardenId, CurrentUserId, bed);
        return CreatedAtAction(nameof(GetById), new { gardenId, id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int gardenId, int id, UpdateBedRequest request)
    {
        var updated = await bedService.UpdateAsync(id, CurrentUserId, request.Name, request.Material, request.ExpectedLifespanYears, request.Notes);
        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:int}/retire")]
    public async Task<IActionResult> Retire(int gardenId, int id, RetireBedRequest request)
    {
        var retired = await bedService.RetireAsync(id, CurrentUserId, request.RetiredDate);
        return retired ? NoContent() : NotFound();
    }

    private static BedResponse Map(RaisedBed b) => new()
    {
        Id = b.Id, GardenId = b.GardenId, Name = b.Name, LengthFt = b.LengthFt,
        WidthFt = b.WidthFt, DepthIn = b.DepthIn, Material = b.Material,
        ExpectedLifespanYears = b.ExpectedLifespanYears, InstalledDate = b.InstalledDate,
        RetiredDate = b.RetiredDate, Notes = b.Notes
    };
}
