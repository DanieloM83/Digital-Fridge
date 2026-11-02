using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IAuthService
{
    Task<(User User, UserSession Session)> ProcessGoogleLoginAsync(
        string googleId,
        string email,
        string? name,
        string? pictureUrl,
        TimeSpan? duration = null);

    Task<bool> ValidateSessionAsync(string sessionId);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task RevokeSessionAsync(string sessionId);
}
