using Digital_Fridge.Models;

namespace Digital_Fridge.Repositories;

public interface IRecipeRepository
{
    Task<IReadOnlyList<Recipe>> GetPublicRecipesAsync();
    Task<Recipe?> GetByIdAsync(Guid recipeId);
    Task<Recipe> AddAsync(Recipe recipe);
    Task UpdateAsync(Recipe recipe);
    Task DeleteAsync(Guid recipeId);
    Task<bool> AnyAsync();
    Task AddRangeAsync(IEnumerable<Recipe> recipes);
    Task<IReadOnlyList<SavedRecipe>> GetSavedRecipesAsync(Guid recipeId, Guid? userId, Guid? familyId);
    Task<bool> ToggleUserBookmarkAsync(Guid recipeId, Guid userId);
    Task<bool> ToggleFamilyBookmarkAsync(Guid recipeId, Guid familyId);
    Task<bool> MakeRecipePublicAsync(Guid recipeId, Guid authorUserId);
}
