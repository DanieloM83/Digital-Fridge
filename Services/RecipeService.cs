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
                r.Ingredients.Any(i => terms.Any(term => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
            )
            .ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid recipeId)
    {
        return await _recipeRepository.GetByIdAsync(recipeId);
    }
}

