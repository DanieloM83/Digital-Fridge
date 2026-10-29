using System.Security.Claims;
using Digital_Fridge.Models;
using Digital_Fridge.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Digital_Fridge.Controllers;

[ApiController]
[Route("api/auth")]
[IgnoreAntiforgeryToken]
public class AuthController : ControllerBase
{
    private readonly ISessionStore _sessionStore;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ISessionStore sessionStore,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _sessionStore = sessionStore;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromForm] GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Credential))
        {
            return BadRequest("Missing credential token.");
        }

        var clientId = _configuration["Authentication:Google:ClientId"];
        GoogleJsonWebSignature.Payload payload;

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings();
            if (!string.IsNullOrWhiteSpace(clientId) && !clientId.Contains("YOUR_GOOGLE_CLIENT_ID"))
            {
                settings.Audience = new[] { clientId };
            }

            payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google JWT token.");
            return Unauthorized("Invalid token.");
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var session = new UserSession
        {
            SessionId = sessionId,
            UserId = payload.Subject,
            Email = payload.Email,
            Name = payload.Name,
            PictureUrl = payload.Picture,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        _sessionStore.CreateSession(session);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, payload.Subject),
            new(ClaimTypes.Name, payload.Name ?? payload.Email),
            new(ClaimTypes.Email, payload.Email),
            new("picture", payload.Picture ?? string.Empty),
            new("session_id", sessionId)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = session.ExpiresAtUtc
            });

        return Redirect("/workspace");
    }

    [HttpGet("logout")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var sessionId = User.FindFirst("session_id")?.Value;
        if (!string.IsNullOrEmpty(sessionId))
        {
            _sessionStore.InvalidateSession(sessionId);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}

