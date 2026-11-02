using Digital_Fridge.Models;

namespace Digital_Fridge.Repositories;

public interface IAuthRepository
{
    Task CreateSessionAsync(UserSession session);
    Task<UserSession?> GetSessionByIdAsync(string sessionId);
    Task InvalidateSessionAsync(string sessionId);
    Task DeleteExpiredSessionsAsync();
}

