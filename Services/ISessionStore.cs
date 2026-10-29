using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface ISessionStore
{
    void CreateSession(UserSession session);
    UserSession? GetSession(string sessionId);
    bool IsSessionValid(string sessionId);
    void InvalidateSession(string sessionId);
}

