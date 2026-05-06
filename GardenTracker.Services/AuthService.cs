using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GardenTracker.Services;

public class AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger) : IAuthService
{
    private readonly string _jwtKey = configuration["Jwt:Key"]!;
    private readonly string _jwtIssuer = configuration["Jwt:Issuer"]!;
    private readonly string _jwtAudience = configuration["Jwt:Audience"]!;

    public async Task<AuthResult?> RegisterAsync(string email, string password, string displayName)
    {
        logger.LogDebug("Registration attempt for email {Email}", email);

        var existing = await userRepository.GetByEmailAsync(email);
        if (existing != null)
        {
            logger.LogInformation("Registration failed — email already in use");
            return null;
        }

        var user = new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };

        user.Id = await userRepository.CreateAsync(user);
        logger.LogInformation("User {UserId} registered successfully", user.Id);
        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        logger.LogDebug("Login attempt for email {Email}", email);

        var user = await userRepository.GetByEmailAsync(email.ToLowerInvariant());
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            logger.LogInformation("Login failed — invalid credentials");
            return null;
        }

        logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResult?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var user = await userRepository.GetByRefreshTokenAsync(tokenHash);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            logger.LogInformation("Token refresh failed — invalid or expired token");
            return null;
        }

        logger.LogInformation("Token refreshed for user {UserId}", user.Id);
        return await GenerateTokensAsync(user);
    }

    public async Task LogoutAsync(int userId)
    {
        await userRepository.UpdateRefreshTokenAsync(userId, null, null);
        logger.LogInformation("User {UserId} logged out", userId);
    }

    private async Task<AuthResult> GenerateTokensAsync(User user)
    {
        var expiry = DateTime.UtcNow.AddHours(1);
        var accessToken = GenerateJwt(user, expiry);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshToken);

        await userRepository.UpdateRefreshTokenAsync(user.Id, refreshTokenHash, DateTime.UtcNow.AddDays(30));

        return new AuthResult(accessToken, refreshToken, expiry, user);
    }

    private string GenerateJwt(User user, DateTime expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName)
        };

        var token = new JwtSecurityToken(_jwtIssuer, _jwtAudience, claims, expires: expiry, signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
