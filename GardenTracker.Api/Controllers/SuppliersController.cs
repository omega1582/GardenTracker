using GardenTracker.Api.DTOs.Requests.Suppliers;
using GardenTracker.Api.DTOs.Responses.Suppliers;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/suppliers")]
public class SuppliersController(ISupplierService supplierService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierResponse>>> GetAll()
    {
        var suppliers = await supplierService.GetAllAsync();
        return Ok(suppliers.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierResponse>> GetById(int id)
    {
        var supplier = await supplierService.GetByIdAsync(id);
        return supplier == null ? NotFound() : Ok(Map(supplier));
    }

    [HttpPost]
    public async Task<ActionResult<SupplierResponse>> Create(CreateSupplierRequest request)
    {
        var supplier = await supplierService.CreateAsync(request.Name, request.SupplierType, request.Website, request.Notes);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, Map(supplier));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateSupplierRequest request)
    {
        var updated = await supplierService.UpdateAsync(id, request.Name, request.SupplierType, request.Website, request.Notes);
        return updated ? NoContent() : NotFound();
    }

    private static SupplierResponse Map(Core.Entities.Supplier s) => new()
    {
        Id = s.Id, Name = s.Name, SupplierType = s.SupplierType, Website = s.Website, Notes = s.Notes
    };
}
