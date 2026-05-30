using GardenTracker.Api.DTOs.Requests.Expenses;
using GardenTracker.Api.DTOs.Responses.Expenses;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Enums;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[Route("api/v1/gardens/{gardenId:int}/seasons/{year:int}/expenses")]
public class ExpensesController(IExpenseService expenseService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponse>>> GetAll(
        int gardenId, int year, [FromQuery] int? bedId, [FromQuery] ExpenseCategory? category)
    {
        var expenses = await expenseService.GetBySeasonAsync(gardenId, year, CurrentUserId, bedId, category);
        return Ok(expenses.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> GetById(int gardenId, int year, int id)
    {
        var expense = await expenseService.GetByIdAsync(id, CurrentUserId);
        return expense == null ? NotFound() : Ok(Map(expense));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create(int gardenId, int year, CreateExpenseRequest request)
    {
        var expense = new Expense
        {
            BedId = request.BedId, SupplierId = request.SupplierId,
            Category = request.Category, Description = request.Description,
            Amount = request.Amount, ExpenseDate = request.ExpenseDate
        };
        var created = await expenseService.CreateAsync(gardenId, year, CurrentUserId, expense);
        return CreatedAtAction(nameof(GetById), new { gardenId, year, id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int gardenId, int year, int id, UpdateExpenseRequest request)
    {
        var expense = new Expense
        {
            BedId = request.BedId, SupplierId = request.SupplierId,
            Category = request.Category, Description = request.Description,
            Amount = request.Amount, ExpenseDate = request.ExpenseDate
        };
        var updated = await expenseService.UpdateAsync(id, CurrentUserId, expense);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int gardenId, int year, int id)
    {
        var deleted = await expenseService.DeleteAsync(id, CurrentUserId);
        return deleted ? NoContent() : NotFound();
    }

    private static ExpenseResponse Map(Expense e) => new()
    {
        Id = e.Id, SeasonId = e.SeasonId, BedId = e.BedId, BedName = e.BedName,
        SupplierId = e.SupplierId, SupplierName = e.SupplierName, Category = e.Category,
        Description = e.Description, Amount = e.Amount, ExpenseDate = e.ExpenseDate
    };
}
