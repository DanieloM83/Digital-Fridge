using Digital_Fridge.Models;

namespace Digital_Fridge.Repositories;

public interface IRecipeGenerationRepository
{
    Task<GeneratedRecipeSession?> GetSessionByUserIdAsync(Guid userId);
    Task<GeneratedRecipeSession> SaveSessionAsync(Guid userId, string prompt, IReadOnlyList<Recipe> recipes);
}
