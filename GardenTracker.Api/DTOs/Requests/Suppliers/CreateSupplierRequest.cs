using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.Suppliers;

public class CreateSupplierRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public SupplierType SupplierType { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    public string? Notes { get; set; }
}
