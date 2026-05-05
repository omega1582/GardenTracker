using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Beds;

public class RetireBedRequest
{
    [Required]
    public DateOnly RetiredDate { get; set; }
}
