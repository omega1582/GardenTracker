using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Responses.Suppliers;

public class SupplierResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SupplierType SupplierType { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
}
