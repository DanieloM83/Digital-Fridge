using Digital_Fridge.Data;
using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Digital_Fridge.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Tests.Integration;

[Trait("Category", "Integration")]
public class RepositoryIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private IDbContextFactory<AppDbContext> _contextFactory = default!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _contextFactory = new TestDbContextFactory(options);
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();

    [Fact]
    public async Task StorageRepository_SaveUpdateAndDeleteFridgeItem_PersistsAcrossContexts()
    {
        await using (var ctx = await _contextFactory.CreateDbContextAsync())
        {
            ctx.Families.Add(new Family { FamilyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Home" });
            ctx.ProductCategories.Add(new ProductCategory { CategoryId = 1, Name = "Dairy & Eggs" });
            await ctx.SaveChangesAsync();
        }

        var repo = new StorageRepository(_contextFactory);
        var familyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var saved = await repo.SaveFridgeItemAsync(
            new FridgeItem { FamilyId = familyId, Quantity = 6, Unit = UnitEnum.Pieces, EntryMethod = EntryMethod.Manual },
            new Product { Name = "Eggs", CategoryId = 1, DefaultUnit = UnitEnum.Pieces });

        var loaded = await repo.GetFridgeItemsByFamilyIdAsync(familyId);
        Assert.Single(loaded);
        Assert.Equal("Eggs", loaded[0].Product?.Name);
        Assert.Equal(6, loaded[0].Quantity);

        var deleted = await repo.DeleteFridgeItemAsync(saved.FridgeItemId);
        Assert.True(deleted);
        Assert.Empty(await repo.GetFridgeItemsByFamilyIdAsync(familyId));
    }

    [Fact]
    public async Task FamilyRepository_JoinAndLeaveToSoloFamily_MovesMembersAndRestoresHead()
    {
        var headId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var targetFamilyId = Guid.NewGuid();

        await using (var ctx = await _contextFactory.CreateDbContextAsync())
        {
            ctx.Families.AddRange(
                new Family { FamilyId = headId, Name = "Original" },
                new Family { FamilyId = targetFamilyId, Name = "Target" });
            ctx.Users.AddRange(
                new User { UserId = headId, FamilyId = headId, GoogleId = "g1", Email = "head@test.com", FullName = "Head", Role = UserRole.FamilyHead },
                new User { UserId = memberId, FamilyId = headId, GoogleId = "g2", Email = "member@test.com", FullName = "Member", Role = UserRole.FamilyMember });
            await ctx.SaveChangesAsync();
        }

        var repo = new FamilyRepository(_contextFactory);
        await repo.JoinFamilyAsync(headId, targetFamilyId);

        var targetFamily = await repo.GetFamilyWithMembersAsync(targetFamilyId);
        Assert.NotNull(targetFamily);
        Assert.Equal(2, targetFamily.Members.Count);
        Assert.All(targetFamily.Members, m => Assert.Equal(UserRole.FamilyMember, m.Role));

        await repo.LeaveToSoloFamilyAsync(memberId);
        var solo = await repo.GetFamilyWithMembersAsync(memberId);
        Assert.NotNull(solo);
        Assert.Single(solo.Members);
        Assert.Equal(UserRole.FamilyHead, solo.Members.Single().Role);
    }

    [Fact]
    public async Task AuthRepository_DeleteExpiredSessions_RemovesOnlyExpiredSessions()
    {
        var userId = Guid.NewGuid();
        await using (var ctx = await _contextFactory.CreateDbContextAsync())
        {
            ctx.Families.Add(new Family { FamilyId = userId, Name = "Solo" });
            ctx.Users.Add(new User { UserId = userId, FamilyId = userId, GoogleId = "g3", Email = "u@test.com" });
            ctx.UserSessions.AddRange(
                new UserSession { SessionId = "expired-1", UserId = userId, ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5) },
                new UserSession { SessionId = "valid-1", UserId = userId, ExpiresAtUtc = DateTime.UtcNow.AddHours(1) });
            await ctx.SaveChangesAsync();
        }

        var repo = new AuthRepository(_contextFactory);
        await repo.DeleteExpiredSessionsAsync();

        Assert.Null(await repo.GetSessionWithUserAsync("expired-1"));
        Assert.NotNull(await repo.GetSessionWithUserAsync("valid-1"));
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken ct = default) =>
            Task.FromResult(new AppDbContext(options));
    }
}
