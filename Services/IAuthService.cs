using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IAuthService
{
    Task<UserSession> CreateUserSessionAsync(string userId, string email, string? name, string? pictureUrl, TimeSpan? duration = null);
    Task<bool> ValidateSessionAsync(string sessionId);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task InvalidateSessionAsync(string sessionId);
}

