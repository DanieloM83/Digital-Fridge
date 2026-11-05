using Digital_Fridge.Models;
using Digital_Fridge.Repositories;

namespace Digital_Fridge.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;

    public RecipeService(IRecipeRepository recipeRepository)
    {
        _recipeRepository = recipeRepository;
    }

    public async Task<IReadOnlyList<Recipe>> GetPublicRecipesAsync(string? searchQuery = null)
    {
        var recipes = await _recipeRepository.GetPublicRecipesAsync();

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return recipes;
        }

        var terms = searchQuery
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        if (terms.Count == 0)
        {
            terms.Add(searchQuery.Trim().ToLowerInvariant());
        }

        return recipes
            .Where(r =>
                terms.Any(term => r.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (r.ShortDescription != null && terms.Any(term => r.ShortDescription.Contains(term, StringComparison.OrdinalIgnoreCase))) ||
                r.Ingredients.Any(i => i.Product != null && terms.Any(term => i.Product.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
            )
            .ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid recipeId)
    {
        return await _recipeRepository.GetByIdAsync(recipeId);
    }

    public async Task<IReadOnlyList<SavedRecipe>> GetSavedRecipesForContextAsync(Guid recipeId, Guid? userId, Guid? familyId)
    {
        return await _recipeRepository.GetSavedRecipesAsync(recipeId, userId, familyId);
    }

    public async Task<bool> ToggleBookmarkAsync(Guid recipeId, Guid userId, Guid? familyId, string target)
    {
        if (target.Equals("family", StringComparison.OrdinalIgnoreCase))
        {
            if (!familyId.HasValue) return false;
            return await _recipeRepository.ToggleFamilyBookmarkAsync(recipeId, familyId.Value);
        }

        return await _recipeRepository.ToggleUserBookmarkAsync(recipeId, userId);
    }

    public async Task<bool> MakeRecipePublicAsync(Guid recipeId, Guid authorUserId)
    {
        return await _recipeRepository.MakeRecipePublicAsync(recipeId, authorUserId);
    }
}
