using GardenTracker.Core.Entities;

namespace GardenTracker.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByRefreshTokenAsync(string refreshTokenHash);
    Task<int> CreateAsync(User user);
    Task UpdateRefreshTokenAsync(int userId, string? refreshToken, DateTime? expiry);
}
