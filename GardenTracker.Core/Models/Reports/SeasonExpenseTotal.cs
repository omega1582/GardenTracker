using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Models.Reports;

public class SeasonExpenseTotal
{
    public ExpenseCategory Category { get; set; }
    public decimal Total { get; set; }
}
