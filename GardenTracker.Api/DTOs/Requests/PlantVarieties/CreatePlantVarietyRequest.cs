using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.PlantVarieties;

public class CreatePlantVarietyRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public GrowthHabit? GrowthHabit { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? SpacingInches { get; set; }
    public SunPreference? SunPreference { get; set; }
    public bool? IsPerennial { get; set; }
}
