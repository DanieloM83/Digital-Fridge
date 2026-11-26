using System.Security.Claims;
using Digital_Fridge.Data;
using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digital_Fridge.Tests.Infrastructure;

/// <summary>
/// Hosts the real ASP.NET Core &amp; Blazor Server app against an isolated in-memory SQLite database.
/// </summary>
public class TestAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public Guid SeededFamilyId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid SeededUserId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid SeededRecipeId { get; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public string SeededSessionId { get; } = "test-session-valid-001";

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        // Trigger server/host creation, then initialize SQLite schema & seed data.
        _ = Services;
        await ResetAndSeedDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services => {
            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddTransient<IStartupFilter, TestAuthStartupFilter>();
        });
    }

    public async Task ResetAndSeedDatabaseAsync()
    {
        var factory = Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var ctx = await factory.CreateDbContextAsync();

        await ctx.Database.EnsureDeletedAsync();
        await ctx.Database.EnsureCreatedAsync();

        var produce = new ProductCategory { Name = "Produce", IconUrl = "bi-flower1" };
        var dairy = new ProductCategory { Name = "Dairy & Eggs", IconUrl = "bi-egg" };
        await ctx.ProductCategories.AddRangeAsync(produce, dairy);
        await ctx.SaveChangesAsync();

        var tomatoes = new Product {
            ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Ripe Tomatoes",
            CategoryId = produce.CategoryId,
            DefaultUnit = UnitEnum.Grams,
            ProcessingLevel = ProcessingLevel.Raw
        };
        var eggs = new Product {
            ProductId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Eggs",
            CategoryId = dairy.CategoryId,
            DefaultUnit = UnitEnum.Pieces,
            ProcessingLevel = ProcessingLevel.Raw
        };
        await ctx.Products.AddRangeAsync(tomatoes, eggs);

        var family = new Family {
            FamilyId = SeededFamilyId,
            Name = "Test Household",
            CreatedAt = DateTime.UtcNow
        };
        var user = new User {
            UserId = SeededUserId,
            FamilyId = SeededFamilyId,
            GoogleId = "test-google-sub",
            Email = "tester@example.com",
            FullName = "Test Chef",
            Role = UserRole.FamilyHead,
            CreatedAt = DateTime.UtcNow,
            JoinedAt = DateTime.UtcNow
        };
        var session = new UserSession {
            SessionId = SeededSessionId,
            UserId = SeededUserId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        var recipe = new Recipe {
            RecipeId = SeededRecipeId,
            Title = "Test Shakshuka",
            ShortDescription = "Warm spiced eggs in tomato sauce",
            Instructions = "1. Simmer tomatoes.\n2. Poach eggs.",
            PrepTimeMinutes = 20,
            Servings = 2,
            IsPublic = true,
            CreatedByUserId = SeededUserId,
            CreatedAt = DateTime.UtcNow,
            Ingredients = [
                new RecipeIngredient { ProductId = tomatoes.ProductId, Quantity = 300, Unit = UnitEnum.Grams },
                new RecipeIngredient { ProductId = eggs.ProductId, Quantity = 4, Unit = UnitEnum.Pieces }
            ]
        };

        await ctx.Families.AddAsync(family);
        await ctx.Users.AddAsync(user);
        await ctx.UserSessions.AddAsync(session);
        await ctx.Recipes.AddAsync(recipe);
        await ctx.SaveChangesAsync();
    }

    /// <summary>
    /// Exposes a test-only endpoint that signs in as the seeded user using the real CookieAuthentication scheme.
    /// </summary>
    private sealed class TestAuthStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app => {
            app.Map("/__test/login", branch => {
                branch.Run(async httpContext => {
                    var factory = httpContext.RequestServices.GetRequiredService<IDbContextFactory<AppDbContext>>();
                    await using var ctx = await factory.CreateDbContextAsync();
                    var user = await ctx.Users.AsNoTracking().FirstAsync();
                    var session = await ctx.UserSessions.AsNoTracking().FirstAsync(s => !s.IsRevoked);

                    var claims = new List<Claim> {
                        new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new(ClaimTypes.Name, user.FullName ?? user.Email),
                        new(ClaimTypes.Email, user.Email),
                        new(ClaimTypes.Role, user.Role.ToString()),
                        new("family_id", user.FamilyId.ToString()),
                        new("session_id", session.SessionId)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await httpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));

                    var redirect = httpContext.Request.Query["redirect"].FirstOrDefault() ?? "/workspace/storage";
                    httpContext.Response.Redirect(redirect);
                });
            });

            next(app);
        };
    }
}
