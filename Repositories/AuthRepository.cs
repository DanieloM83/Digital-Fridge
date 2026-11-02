using Digital_Fridge.Data;
using Digital_Fridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AuthRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<User?> GetUserByGoogleIdAsync(string googleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .AsNoTracking()
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .AsNoTracking()
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task CreateUserWithFamilyAsync(User user, Family family)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.Families.AddAsync(family);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task CreateSessionAsync(UserSession session)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.UserSessions.AddAsync(session);
        await context.SaveChangesAsync();
    }

    public async Task<UserSession?> GetSessionWithUserAsync(string sessionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
                .ThenInclude(u => u!.Family)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task RevokeSessionAsync(string sessionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var session = await context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        if (session != null)
        {
            session.IsRevoked = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredSessionsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;
        await context.UserSessions
            .Where(s => s.ExpiresAtUtc <= now || s.IsRevoked)
            .ExecuteDeleteAsync();
    }
}
