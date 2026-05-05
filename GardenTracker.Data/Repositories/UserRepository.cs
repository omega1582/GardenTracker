using Dapper;
using GardenTracker.Core.Entities;
using GardenTracker.Core.Interfaces.Repositories;

namespace GardenTracker.Data.Repositories;

public class UserRepository(IConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshTokenHash)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE RefreshToken = @RefreshToken AND RefreshTokenExpiry > @Now",
            new { RefreshToken = refreshTokenHash, Now = DateTime.UtcNow });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO Users (Email, PasswordHash, DisplayName, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Email, @PasswordHash, @DisplayName, @CreatedAt)
            """,
            user);
    }

    public async Task UpdateRefreshTokenAsync(int userId, string? refreshToken, DateTime? expiry)
    {
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET RefreshToken = @RefreshToken, RefreshTokenExpiry = @Expiry WHERE Id = @UserId",
            new { UserId = userId, RefreshToken = refreshToken, Expiry = expiry });
    }
}
