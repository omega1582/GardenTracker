using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Responses.Expenses;

public class ExpenseResponse
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int? BedId { get; set; }
    public string? BedName { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public ExpenseCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
}
