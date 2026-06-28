using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Beds;

public class UpdateBedRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(0.1, 1000)]
    public decimal LengthFt { get; set; }

    [Required, Range(0.1, 1000)]
    public decimal WidthFt { get; set; }

    [Required, Range(1, 120)]
    public decimal DepthIn { get; set; }

    [MaxLength(100)]
    public string? Material { get; set; }

    [Range(1, 100)]
    public int ExpectedLifespanYears { get; set; } = 10;

    [Required]
    public DateOnly InstalledDate { get; set; }

    public string? Notes { get; set; }
}
