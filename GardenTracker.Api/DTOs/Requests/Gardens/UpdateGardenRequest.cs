using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Gardens;

public class UpdateGardenRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Location { get; set; }

    public string? Notes { get; set; }
}
