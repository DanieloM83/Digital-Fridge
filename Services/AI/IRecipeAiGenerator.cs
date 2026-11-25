using Digital_Fridge.Models;

namespace Digital_Fridge.Services.AI;

public interface IRecipeAiGenerator
{
    Task<IReadOnlyList<Recipe>> GenerateRecipesAsync(Guid userId, string prompt, IReadOnlyList<FridgeItem> availableInventory);
}
