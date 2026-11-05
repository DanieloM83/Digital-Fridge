using Digital_Fridge.Models;
using Digital_Fridge.Repositories;

namespace Digital_Fridge.Services;

public class FamilyService(IFamilyRepository familyRepository) : IFamilyService
{
    public Task<Family?> GetFamilyWithMembersAsync(Guid familyId) =>
        familyRepository.GetFamilyWithMembersAsync(familyId);

    public async Task<bool> JoinFamilyAsync(Guid userId, Guid targetFamilyId)
    {
        var target = await familyRepository.GetFamilyByIdAsync(targetFamilyId);
        if (target is null) return false;

        await familyRepository.JoinFamilyAsync(userId, targetFamilyId);
        return true;
    }

    public Task LeaveToSoloFamilyAsync(Guid userId) =>
        familyRepository.LeaveToSoloFamilyAsync(userId);

    public Task KickMemberAsync(Guid targetUserId) =>
        familyRepository.KickMemberAsync(targetUserId);
}

