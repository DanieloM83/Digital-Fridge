using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IFamilyService
{
    Task<Family?> GetFamilyWithMembersAsync(Guid familyId);

    // Returns false if targetFamilyId does not exist.
    Task<bool> JoinFamilyAsync(Guid userId, Guid targetFamilyId);

    Task LeaveToSoloFamilyAsync(Guid userId);

    Task KickMemberAsync(Guid targetUserId);
}

