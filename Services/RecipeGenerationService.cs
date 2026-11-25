using Digital_Fridge.Models;
using Digital_Fridge.Repositories;
using Digital_Fridge.Services.AI;

namespace Digital_Fridge.Services;

public class RecipeGenerationService : IRecipeGenerationService
{
    private readonly IRecipeGenerationRepository _generationRepository;
    private readonly IStorageService _storageService;
    private readonly IAuthRepository _authRepository;
    private readonly IRecipeAiGenerator _aiGenerator;

    public RecipeGenerationService(
        IRecipeGenerationRepository generationRepository,
        IStorageService storageService,
        IAuthRepository authRepository,
        IRecipeAiGenerator aiGenerator)
    {
        _generationRepository = generationRepository;
        _storageService = storageService;
        _authRepository = authRepository;
        _aiGenerator = aiGenerator;
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

        // Delegate generation to the swappable AI provider (Gemini, OpenAI, or Fallback)
        var generated = await _aiGenerator.GenerateRecipesAsync(userId, userPrompt, fridgeItems);

        return await _generationRepository.SaveSessionAsync(userId, userPrompt, generated);
    }
}
