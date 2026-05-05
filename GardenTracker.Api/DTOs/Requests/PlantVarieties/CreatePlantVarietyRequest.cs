using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.PlantVarieties;

public class CreatePlantVarietyRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
