using Digital_Fridge.Data;
using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Repositories;

public class FamilyRepository(IDbContextFactory<AppDbContext> contextFactory) : IFamilyRepository
{
    public async Task<Family?> GetFamilyByIdAsync(Guid familyId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Families.AsNoTracking().FirstOrDefaultAsync(f => f.FamilyId == familyId);
    }

    public async Task<Family?> GetFamilyWithMembersAsync(Guid familyId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Families
            .AsNoTracking()
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.FamilyId == familyId);
    }

    public async Task JoinFamilyAsync(Guid userId, Guid targetFamilyId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var user = await ctx.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                   ?? throw new InvalidOperationException("User not found.");

        if (user.FamilyId == targetFamilyId) return;

        if (user.Role == UserRole.FamilyHead)
        {
            // Move all current family members (except the head) to the target family as FamilyMembers
            var members = await ctx.Users
                .Where(u => u.FamilyId == user.FamilyId && u.UserId != userId)
                .ToListAsync();

            foreach (var member in members)
            {
                member.FamilyId = targetFamilyId;
                member.JoinedAt = DateTime.UtcNow;
                // Members keep FamilyMember role
            }
        }

        user.FamilyId = targetFamilyId;
        user.Role = UserRole.FamilyMember;
        user.JoinedAt = DateTime.UtcNow;

        await ctx.SaveChangesAsync();
    }

    public async Task LeaveToSoloFamilyAsync(Guid userId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var user = await ctx.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                   ?? throw new InvalidOperationException("User not found.");

        await EnsureSoloFamilyExistsAsync(ctx, user);

        user.FamilyId = userId;
        user.Role = UserRole.FamilyHead;
        user.JoinedAt = DateTime.UtcNow;

        await ctx.SaveChangesAsync();
    }

    public async Task KickMemberAsync(Guid targetUserId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var user = await ctx.Users.FirstOrDefaultAsync(u => u.UserId == targetUserId)
                   ?? throw new InvalidOperationException("User not found.");

        await EnsureSoloFamilyExistsAsync(ctx, user);

        user.FamilyId = targetUserId;
        user.Role = UserRole.FamilyHead;
        user.JoinedAt = DateTime.UtcNow;

        await ctx.SaveChangesAsync();
    }

    private static async Task EnsureSoloFamilyExistsAsync(AppDbContext ctx, User user)
    {
        var soloFamilyId = user.UserId;
        var exists = await ctx.Families.AnyAsync(f => f.FamilyId == soloFamilyId);
        if (!exists)
        {
            var name = string.IsNullOrWhiteSpace(user.FullName)
                ? user.Email
                : user.FullName;

            await ctx.Families.AddAsync(new Family {
                FamilyId = soloFamilyId,
                Name = $"{name}'s Household",
                CreatedAt = DateTime.UtcNow
            });
            // SaveChanges is called by the caller after mutating the user
        }
    }
}
