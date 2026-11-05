using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IRecipeService
{
    Task<IReadOnlyList<Recipe>> GetPublicRecipesAsync(string? searchQuery = null);
    Task<Recipe?> GetRecipeByIdAsync(Guid recipeId);
    Task<IReadOnlyList<SavedRecipe>> GetSavedRecipesForContextAsync(Guid recipeId, Guid? userId, Guid? familyId);
    Task<bool> ToggleBookmarkAsync(Guid recipeId, Guid userId, Guid? familyId, string target);
    Task<bool> MakeRecipePublicAsync(Guid recipeId, Guid authorUserId);
}

