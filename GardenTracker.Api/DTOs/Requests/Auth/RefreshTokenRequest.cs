using System.ComponentModel.DataAnnotations;

namespace GardenTracker.Api.DTOs.Requests.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
