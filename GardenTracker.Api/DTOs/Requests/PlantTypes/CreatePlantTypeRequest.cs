using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.PlantTypes;

public class CreatePlantTypeRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public GrowthHabit? GrowthHabit { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? SpacingInches { get; set; }
    public SunPreference? SunPreference { get; set; }
    public bool? IsPerennial { get; set; }
}
