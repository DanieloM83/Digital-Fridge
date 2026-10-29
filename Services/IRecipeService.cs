using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IRecipeService
{
    Task<IReadOnlyList<Recipe>> GetPublicRecipesAsync(string? searchQuery = null);
    Task<Recipe?> GetRecipeByIdAsync(Guid recipeId);
}

