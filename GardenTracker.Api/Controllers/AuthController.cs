using GardenTracker.Api.DTOs.Requests.Auth;
using GardenTracker.Api.DTOs.Responses.Auth;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GardenTracker.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request.Email, request.Password, request.DisplayName);
        if (result == null) return Conflict("Email already registered.");
        return Ok(MapResponse(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request.Email, request.Password);
        if (result == null) return Unauthorized("Invalid credentials.");
        return Ok(MapResponse(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        if (result == null) return Unauthorized("Invalid or expired refresh token.");
        return Ok(MapResponse(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        if (result == null) return Unauthorized("Invalid or expired refresh token.");
        await authService.LogoutAsync(result.User.Id);
        return NoContent();
    }

    private static AuthResponse MapResponse(Core.Interfaces.Services.AuthResult result) => new()
    {
        AccessToken = result.AccessToken,
        RefreshToken = result.RefreshToken,
        ExpiresAt = result.ExpiresAt,
        UserId = result.User.Id,
        DisplayName = result.User.DisplayName
    };
}
