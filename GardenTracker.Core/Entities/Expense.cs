using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class Expense
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int? BedId { get; set; }
    public int? SupplierId { get; set; }
    public ExpenseCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }

    public Season Season { get; set; } = null!;
    public RaisedBed? Bed { get; set; }
    public Supplier? Supplier { get; set; }
}
