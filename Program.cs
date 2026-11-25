using Digital_Fridge.Components;
using Digital_Fridge.Data;
using Digital_Fridge.Repositories;
using Digital_Fridge.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options => {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB for image captures
    });

builder.Services.AddControllers();

builder.Services.AddHttpClient();

// Database & Persistence Layer
builder.Services.AddPostgresDbContext(builder.Configuration);
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped<IRecipeGenerationRepository, RecipeGenerationRepository>();

// AI Provider Adapters (Swappable with zero coupling to UI)
builder.Services.AddScoped<Digital_Fridge.Services.AI.IProductVisionService, Digital_Fridge.Services.AI.GeminiProductVisionService>();
builder.Services.AddScoped<Digital_Fridge.Services.AI.IRecipeImageGenerator, Digital_Fridge.Services.AI.GeminiRecipeImageGenerator>();
builder.Services.AddScoped<Digital_Fridge.Services.AI.IRecipeAiGenerator, Digital_Fridge.Services.AI.GeminiRecipeAiGenerator>();

// Core Business Services Layer
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IRecipeGenerationService, RecipeGenerationService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.Name = "DigitalFridge.Auth";
        options.LoginPath = "/";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents {
            OnValidatePrincipal = async context => {
                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var sessionId = context.Principal?.FindFirst("session_id")?.Value;
                if (string.IsNullOrEmpty(sessionId) || !await authService.ValidateSessionAsync(sessionId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

var supportedCultures = new[] { "uk", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("uk")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapGet("/api/culture/set", (string culture, string? redirectUri, HttpContext httpContext) =>
{
    var normalized = culture == "en" ? "en" : "uk";
    httpContext.Response.Cookies.Append(
        Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
        Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
            new Microsoft.AspNetCore.Localization.RequestCulture(normalized)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

    var target = !string.IsNullOrWhiteSpace(redirectUri) && Uri.TryCreate(redirectUri, UriKind.Relative, out _)
        ? redirectUri
        : "/";
    return Results.LocalRedirect(target);
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Seed initial recipes & ensure DB schema
try
{
    await DbInitializer.InitializeDatabaseAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Could not initialize database on startup. Ensure PostgreSQL credentials are configured.");
}

app.Run();

public partial class Program { }
