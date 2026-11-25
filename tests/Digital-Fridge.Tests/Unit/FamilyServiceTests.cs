using Digital_Fridge.Models;
using Digital_Fridge.Repositories;
using Digital_Fridge.Services;

namespace Digital_Fridge.Tests.Unit;

[Trait("Category", "Unit")]
public class FamilyServiceTests
{
    [Fact]
    public async Task JoinFamilyAsync_WhenTargetFamilyDoesNotExist_ReturnsFalseWithoutJoining()
    {
        var repo = new FakeFamilyRepository(existingFamily: null);
        var service = new FamilyService(repo);

        var joined = await service.JoinFamilyAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(joined);
        Assert.False(repo.JoinCalled);
    }

    [Fact]
    public async Task JoinFamilyAsync_WhenTargetFamilyExists_JoinsAndReturnsTrue()
    {
        var familyId = Guid.NewGuid();
        var repo = new FakeFamilyRepository(new Family { FamilyId = familyId, Name = "Target" });
        var service = new FamilyService(repo);

        var joined = await service.JoinFamilyAsync(Guid.NewGuid(), familyId);

        Assert.True(joined);
        Assert.True(repo.JoinCalled);
    }

    private sealed class FakeFamilyRepository(Family? existingFamily) : IFamilyRepository
    {
        public bool JoinCalled { get; private set; }

        public Task<Family?> GetFamilyByIdAsync(Guid familyId) =>
            Task.FromResult(existingFamily?.FamilyId == familyId ? existingFamily : null);

        public Task<Family?> GetFamilyWithMembersAsync(Guid familyId) => Task.FromResult(existingFamily);

        public Task JoinFamilyAsync(Guid userId, Guid targetFamilyId)
        {
            JoinCalled = true;
            return Task.CompletedTask;
        }

        public Task LeaveToSoloFamilyAsync(Guid userId) => Task.CompletedTask;
        public Task KickMemberAsync(Guid targetUserId) => Task.CompletedTask;
    }
}
