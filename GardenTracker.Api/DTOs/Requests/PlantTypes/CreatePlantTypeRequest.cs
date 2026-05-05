using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.PlantTypes;

public class CreatePlantTypeRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
