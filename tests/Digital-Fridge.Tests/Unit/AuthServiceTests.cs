using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Digital_Fridge.Repositories;
using Digital_Fridge.Services;

namespace Digital_Fridge.Tests.Unit;

[Trait("Category", "Unit")]
public class AuthServiceTests
{
    [Fact]
    public async Task ProcessGoogleLoginAsync_NewUser_CreatesSoloFamilyAsFamilyHeadAndSession()
    {
        var repo = new FakeAuthRepository();
        var service = new AuthService(repo);

        var (user, session) = await service.ProcessGoogleLoginAsync(
            "google-123",
            "alice@example.com",
            "Alice",
            "https://img/alice.png",
            TimeSpan.FromHours(2));

        Assert.Equal(user.UserId, user.FamilyId);
        Assert.Equal(UserRole.FamilyHead, user.Role);
        Assert.Equal("Alice's Household", repo.CreatedFamily?.Name);
        Assert.False(session.IsRevoked);
        Assert.Equal(user.UserId, session.UserId);
    }

    [Fact]
    public async Task ProcessGoogleLoginAsync_ExistingUserWithChangedName_UpdatesUser()
    {
        var existingUser = new User {
            UserId = Guid.NewGuid(),
            FamilyId = Guid.NewGuid(),
            GoogleId = "google-123",
            Email = "old@example.com",
            FullName = "Old Name",
            Role = UserRole.FamilyMember
        };
        var repo = new FakeAuthRepository(existingUser);
        var service = new AuthService(repo);

        var (user, _) = await service.ProcessGoogleLoginAsync("google-123", "new@example.com", "New Name", null);

        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("New Name", user.FullName);
        Assert.Equal(UserRole.FamilyMember, user.Role);
        Assert.True(repo.UserUpdated);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateSessionAsync_BlankSessionId_ReturnsFalse(string? sessionId)
    {
        var service = new AuthService(new FakeAuthRepository());
        Assert.False(await service.ValidateSessionAsync(sessionId!));
    }

    [Fact]
    public async Task ValidateSessionAsync_RevokedOrExpired_ReturnsFalse()
    {
        var repo = new FakeAuthRepository();
        repo.Sessions["revoked"] = new UserSession { SessionId = "revoked", IsRevoked = true, ExpiresAtUtc = DateTime.UtcNow.AddDays(1) };
        repo.Sessions["expired"] = new UserSession { SessionId = "expired", IsRevoked = false, ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1) };
        repo.Sessions["active"] = new UserSession { SessionId = "active", IsRevoked = false, ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30) };

        var service = new AuthService(repo);

        Assert.False(await service.ValidateSessionAsync("revoked"));
        Assert.False(await service.ValidateSessionAsync("expired"));
        Assert.True(await service.ValidateSessionAsync("active"));
    }

    private sealed class FakeAuthRepository(User? existingUser = null) : IAuthRepository
    {
        public Family? CreatedFamily { get; private set; }
        public bool UserUpdated { get; private set; }
        public Dictionary<string, UserSession> Sessions { get; } = [];

        public Task<User?> GetUserByGoogleIdAsync(string googleId) =>
            Task.FromResult(existingUser?.GoogleId == googleId ? existingUser : null);

        public Task<User?> GetUserByIdAsync(Guid userId) =>
            Task.FromResult(existingUser?.UserId == userId ? existingUser : null);

        public Task CreateUserWithFamilyAsync(User user, Family family)
        {
            CreatedFamily = family;
            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(User user)
        {
            UserUpdated = true;
            return Task.CompletedTask;
        }

        public Task CreateSessionAsync(UserSession session)
        {
            Sessions[session.SessionId] = session;
            return Task.CompletedTask;
        }

        public Task<UserSession?> GetSessionWithUserAsync(string sessionId) =>
            Task.FromResult(Sessions.GetValueOrDefault(sessionId));

        public Task RevokeSessionAsync(string sessionId)
        {
            if (Sessions.TryGetValue(sessionId, out var s)) s.IsRevoked = true;
            return Task.CompletedTask;
        }

        public Task DeleteExpiredSessionsAsync() => Task.CompletedTask;
    }
}
