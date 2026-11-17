using Digital_Fridge.Models;
using Digital_Fridge.Repositories;

namespace Digital_Fridge.Services;

public class MockRecipeGenerationService : IRecipeGenerationService
{
    private readonly IRecipeService _recipeService;
    private readonly IStorageService _storageService;
    private readonly IAuthRepository _authRepository;

    public MockRecipeGenerationService(
        IRecipeService recipeService,
        IStorageService storageService,
        IAuthRepository authRepository)
    {
        _recipeService = recipeService;
        _storageService = storageService;
        _authRepository = authRepository;
    }

    public async Task<IReadOnlyList<Recipe>> GetRecentGeneratedRecipesAsync(Guid userId)
    {
        // Temporary stub until database migration is applied
        var publicRecipes = await _recipeService.GetPublicRecipesAsync();
        return publicRecipes.Take(3).ToList();
    }

    public async Task<IReadOnlyList<Recipe>> GenerateRecipesAsync(Guid userId, string userPrompt)
    {
        // Temporary stub until database migration is applied
        var publicRecipes = await _recipeService.GetPublicRecipesAsync(userPrompt);
        if (publicRecipes.Count < 3)
        {
            publicRecipes = await _recipeService.GetPublicRecipesAsync();
        }
        return publicRecipes.Take(3).ToList();
    }
}
