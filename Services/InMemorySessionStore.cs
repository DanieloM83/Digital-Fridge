using System.Collections.Concurrent;
using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();

    public void CreateSession(UserSession session)
    {
        _sessions[session.SessionId] = session;
    }

    public UserSession? GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            if (DateTime.UtcNow <= session.ExpiresAtUtc)
            {
                return session;
            }

            // Lazy cleanup on expired session
            _sessions.TryRemove(sessionId, out _);
        }

        return null;
    }

    public bool IsSessionValid(string sessionId)
    {
        return GetSession(sessionId) != null;
    }

    public void InvalidateSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }
}

