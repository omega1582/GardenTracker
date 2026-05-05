using GardenTracker.Core.Enums;

namespace GardenTracker.Core.Entities;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SupplierType SupplierType { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }

    public ICollection<BedPlanting> Plantings { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
}
