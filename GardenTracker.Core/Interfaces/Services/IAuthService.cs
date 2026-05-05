using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Services;

public record AuthResult(string AccessToken, string RefreshToken, DateTime ExpiresAt, User User);

public interface IAuthService
{
    Task<AuthResult?> RegisterAsync(string email, string password, string displayName);
    Task<AuthResult?> LoginAsync(string email, string password);
    Task<AuthResult?> RefreshAsync(string refreshToken);
    Task LogoutAsync(int userId);
}
