using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Seasons;

public class CreateSeasonRequest
{
    [Required, Range(2000, 2100)]
    public int Year { get; set; }

    public string? Notes { get; set; }
}
