using Digital_Fridge.Components;
using Digital_Fridge.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DigitalFridge.Auth";
        options.LoginPath = "/";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var sessionStore = context.HttpContext.RequestServices.GetRequiredService<ISessionStore>();
                var sessionId = context.Principal?.FindFirst("session_id")?.Value;
                if (string.IsNullOrEmpty(sessionId) || !sessionStore.IsSessionValid(sessionId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
