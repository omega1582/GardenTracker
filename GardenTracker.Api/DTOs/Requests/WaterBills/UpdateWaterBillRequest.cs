using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.WaterBills;

public class UpdateWaterBillRequest
{
    [Required, Range(0.0001, 100000)]
    public decimal UsageCcf { get; set; }

    [Required, Range(0.01, 10000000)]
    public decimal UsageGallons { get; set; }

    [Required, Range(0.01, 100000)]
    public decimal TotalCost { get; set; }

    [Required]
    public bool IsGardenActive { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
