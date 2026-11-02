using Digital_Fridge.Models;
using Digital_Fridge.Repositories;

namespace Digital_Fridge.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;

    public AuthService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<UserSession> CreateUserSessionAsync(
        string userId,
        string email,
        string? name,
        string? pictureUrl,
        TimeSpan? duration = null)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var sessionDuration = duration ?? TimeSpan.FromDays(7);

        var session = new UserSession {
            SessionId = sessionId,
            UserId = userId,
            Email = email,
            Name = name,
            PictureUrl = pictureUrl,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(sessionDuration)
        };

        await _authRepository.CreateSessionAsync(session);
        return session;
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return false;
        }

        var session = await _authRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        if (DateTime.UtcNow > session.ExpiresAtUtc)
        {
            await _authRepository.InvalidateSessionAsync(sessionId);
            return false;
        }

        return true;
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        var session = await _authRepository.GetSessionByIdAsync(sessionId);
        if (session != null && DateTime.UtcNow > session.ExpiresAtUtc)
        {
            await _authRepository.InvalidateSessionAsync(sessionId);
            return null;
        }

        return session;
    }

    public async Task InvalidateSessionAsync(string sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await _authRepository.InvalidateSessionAsync(sessionId);
        }
    }
}

