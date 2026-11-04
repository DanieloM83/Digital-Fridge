using Digital_Fridge.Models;

namespace Digital_Fridge.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByGoogleIdAsync(string googleId);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task CreateUserWithFamilyAsync(User user, Family family);
    Task UpdateUserAsync(User user);
    Task CreateSessionAsync(UserSession session);
    Task<UserSession?> GetSessionWithUserAsync(string sessionId);
    Task RevokeSessionAsync(string sessionId);
    Task DeleteExpiredSessionsAsync();
}
