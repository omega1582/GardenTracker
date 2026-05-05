using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Beds;

public class UpdateBedRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Material { get; set; }

    [Range(1, 100)]
    public int ExpectedLifespanYears { get; set; } = 10;

    public string? Notes { get; set; }
}
