using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;
using GardenTracker.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GardenTracker.Services;

public class AuthService(IUserRepository userRepository, IConfiguration configuration) : IAuthService
{
    private readonly string _jwtKey = configuration["Jwt:Key"]!;
    private readonly string _jwtIssuer = configuration["Jwt:Issuer"]!;
    private readonly string _jwtAudience = configuration["Jwt:Audience"]!;

    public async Task<AuthResult?> RegisterAsync(string email, string password, string displayName)
    {
        var existing = await userRepository.GetByEmailAsync(email);
        if (existing != null) return null;

        var user = new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };

        user.Id = await userRepository.CreateAsync(user);
        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email.ToLowerInvariant());
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResult?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var user = await userRepository.GetByRefreshTokenAsync(tokenHash);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return null;

        return await GenerateTokensAsync(user);
    }

    public async Task LogoutAsync(int userId)
    {
        await userRepository.UpdateRefreshTokenAsync(userId, null, null);
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
