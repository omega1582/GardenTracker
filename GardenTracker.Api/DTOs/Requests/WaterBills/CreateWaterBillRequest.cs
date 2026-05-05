using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.WaterBills;

public class CreateWaterBillRequest
{
    [Required, Range(2000, 2100)]
    public int Year { get; set; }

    [Required, Range(1, 12)]
    public int Month { get; set; }

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
