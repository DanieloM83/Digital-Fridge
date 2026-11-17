using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Digital_Fridge.Repositories;

namespace Digital_Fridge.Services;

public class RecipeGenerationService : IRecipeGenerationService
{
    private readonly IRecipeGenerationRepository _generationRepository;
    private readonly IStorageService _storageService;
    private readonly IAuthRepository _authRepository;

    public RecipeGenerationService(
        IRecipeGenerationRepository generationRepository,
        IStorageService storageService,
        IAuthRepository authRepository)
    {
        _generationRepository = generationRepository;
        _storageService = storageService;
        _authRepository = authRepository;
    }

    public async Task<GeneratedRecipeSession?> GetRecentSessionAsync(Guid userId)
    {
        return await _generationRepository.GetSessionByUserIdAsync(userId);
    }

    public async Task<GeneratedRecipeSession> GenerateRecipesAsync(Guid userId, string userPrompt)
    {
        var user = await _authRepository.GetUserByIdAsync(userId);
        IReadOnlyList<FridgeItem> fridgeItems = [];
        if (user != null)
        {
            fridgeItems = await _storageService.GetFamilyFridgeItemsAsync(user.FamilyId);
        }

        // Generate 3 recipes incorporating user's available ingredients and prompt
        var generated = CreateContextualRecipes(userId, userPrompt, fridgeItems);

        return await _generationRepository.SaveSessionAsync(userId, userPrompt, generated);
    }

    private static List<Recipe> CreateContextualRecipes(Guid userId, string prompt, IReadOnlyList<FridgeItem> fridgeItems)
    {
        var availableProductNames = fridgeItems
            .Where(f => f.Product != null)
            .Select(f => f.Product!.Name)
            .Distinct()
            .Take(4)
            .ToList();

        var staple1 = availableProductNames.Count > 0 ? availableProductNames[0] : "Eggs";
        var staple2 = availableProductNames.Count > 1 ? availableProductNames[1] : "Tomatoes";

        return
        [
            new Recipe
            {
                RecipeId = Guid.NewGuid(),
                CreatedByUserId = userId,
                Title = $"Chef's Pan-Seared {staple1} Skillet",
                ShortDescription = $"A quick savory dish built around {staple1.ToLowerInvariant()} tailored to your request: \"{prompt}\".",
                Instructions = $"1. Prep and slice {staple1.ToLowerInvariant()}.\n2. Heat olive oil in a skillet over medium heat.\n3. Sauté with garlic and herbs until fragrant and golden.\n4. Season with sea salt and cracked black pepper, then serve warm.",
                ImageUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=800&auto=format&fit=crop",
                IsPublic = false,
                PrepTimeMinutes = 20,
                Servings = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Recipe
            {
                RecipeId = Guid.NewGuid(),
                CreatedByUserId = userId,
                Title = $"Cozy {staple2} Medley Bowl",
                ShortDescription = $"A wholesome warm bowl featuring {staple2.ToLowerInvariant()} inspired by your prompt: \"{prompt}\".",
                Instructions = $"1. Rinse and slice {staple2.ToLowerInvariant()} into bite-sized portions.\n2. Toss gently with olive oil, salt, and smoked paprika.\n3. Roast or simmer gently for 15 minutes.\n4. Layer into warm bowls and garnish with fresh herbs.",
                ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?q=80&w=800&auto=format&fit=crop",
                IsPublic = false,
                PrepTimeMinutes = 25,
                Servings = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Recipe
            {
                RecipeId = Guid.NewGuid(),
                CreatedByUserId = userId,
                Title = $"Crisp {staple1} & Herb Garden Toss",
                ShortDescription = $"A vibrant, balanced recipe designed for effortless meal prep with what you have on hand.",
                Instructions = $"1. Lightly sear or warm {staple1.ToLowerInvariant()} in a pan.\n2. Combine in a large serving bowl with mixed greens or available pantry items.\n3. Whisk olive oil, lemon juice, salt, and pepper for dressing.\n4. Drizzle and toss immediately before serving.",
                ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?q=80&w=800&auto=format&fit=crop",
                IsPublic = false,
                PrepTimeMinutes = 15,
                Servings = 2,
                CreatedAt = DateTime.UtcNow
            }
        ];
    }
}
