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

    public async Task CreateSessionAsync(UserSession session)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.UserSessions.Add(session);
        await context.SaveChangesAsync();
    }

    public async Task<UserSession?> GetSessionByIdAsync(string sessionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task InvalidateSessionAsync(string sessionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var session = await context.UserSessions.FindAsync(sessionId);
        if (session != null)
        {
            context.UserSessions.Remove(session);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredSessionsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;
        await context.UserSessions
            .Where(s => s.ExpiresAtUtc <= now)
            .ExecuteDeleteAsync();
    }
}

