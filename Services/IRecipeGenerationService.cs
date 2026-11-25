using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IRecipeGenerationService
{
    Task<GeneratedRecipeSession?> GetRecentSessionAsync(Guid userId);
    Task<GeneratedRecipeSession> GenerateRecipesAsync(Guid userId, string userPrompt);
}
