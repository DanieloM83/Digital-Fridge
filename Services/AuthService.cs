using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Digital_Fridge.Repositories;

namespace Digital_Fridge.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;

    public AuthService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<(User User, UserSession Session)> ProcessGoogleLoginAsync(
        string googleId,
        string email,
        string? name,
        string? pictureUrl,
        TimeSpan? duration = null)
    {
        var user = await _authRepository.GetUserByGoogleIdAsync(googleId);

        if (user == null)
        {
            // First-time login: create household with user_id == family_id
            var familyAndUserId = Guid.NewGuid();
            var householdName = string.IsNullOrWhiteSpace(name) ? "My Household" : $"{name.Trim()}'s Household";

            var family = new Family {
                FamilyId = familyAndUserId,
                Name = householdName,
                CreatedAt = DateTime.UtcNow
            };

            user = new User {
                UserId = familyAndUserId,
                FamilyId = familyAndUserId,
                GoogleId = googleId,
                Email = email,
                FullName = name,
                AvatarUrl = pictureUrl,
                Role = UserRole.FamilyHead,
                CreatedAt = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            };

            await _authRepository.CreateUserWithFamilyAsync(user, family);
        }
        else
        {
            // Update profile metadata if changed
            var hasChanges = false;
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = email;
                hasChanges = true;
            }

            if (user.FullName != name && !string.IsNullOrEmpty(name))
            {
                user.FullName = name;
                hasChanges = true;
            }

            if (user.AvatarUrl != pictureUrl && !string.IsNullOrEmpty(pictureUrl))
            {
                user.AvatarUrl = pictureUrl;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _authRepository.UpdateUserAsync(user);
            }
        }

        // Create new session
        var sessionId = Guid.NewGuid().ToString("N");
        var sessionDuration = duration ?? TimeSpan.FromDays(7);

        var session = new UserSession {
            SessionId = sessionId,
            UserId = user.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(sessionDuration),
            IsRevoked = false
        };

        await _authRepository.CreateSessionAsync(session);
        return (user, session);
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return false;
        }

        var session = await _authRepository.GetSessionWithUserAsync(sessionId);
        if (session == null || session.IsRevoked || DateTime.UtcNow > session.ExpiresAtUtc)
        {
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

        var session = await _authRepository.GetSessionWithUserAsync(sessionId);
        if (session == null || session.IsRevoked || DateTime.UtcNow > session.ExpiresAtUtc)
        {
            return null;
        }

        return session;
    }

    public async Task RevokeSessionAsync(string sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await _authRepository.RevokeSessionAsync(sessionId);
        }
    }
}
