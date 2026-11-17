using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IRecipeGenerationService
{
    Task<IReadOnlyList<Recipe>> GetRecentGeneratedRecipesAsync(Guid userId);
    Task<IReadOnlyList<Recipe>> GenerateRecipesAsync(Guid userId, string userPrompt);
}
