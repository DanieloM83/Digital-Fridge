using Digital_Fridge.Models;

namespace Digital_Fridge.Repositories;

public interface IFamilyRepository
{
    Task<Family?> GetFamilyByIdAsync(Guid familyId);
    Task<Family?> GetFamilyWithMembersAsync(Guid familyId);

    // Move the user (and all current family members if user is FamilyHead) into targetFamilyId.
    Task JoinFamilyAsync(Guid userId, Guid targetFamilyId);

    /// Move the user back to their personal solo family (FamilyId == UserId), role = FamilyHead.
    /// Creates the solo family if it no longer exists.
    Task LeaveToSoloFamilyAsync(Guid userId);

    /// Move targetUserId to their personal solo family. Caller must be FamilyHead of the same family.
    Task KickMemberAsync(Guid targetUserId);
}

