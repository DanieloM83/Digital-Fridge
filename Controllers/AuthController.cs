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
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authService = authService;
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

        var session = await _authService.CreateUserSessionAsync(
            payload.Subject,
            payload.Email,
            payload.Name,
            payload.Picture,
            TimeSpan.FromDays(7));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, payload.Subject),
            new(ClaimTypes.Name, payload.Name ?? payload.Email),
            new(ClaimTypes.Email, payload.Email),
            new("picture", payload.Picture ?? string.Empty),
            new("session_id", session.SessionId)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties {
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
            await _authService.InvalidateSessionAsync(sessionId);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
