namespace Digital_Fridge.Services.AI;

/// <summary>
/// Provider-agnostic adapter for generating a dish image for a recipe.
/// </summary>
public interface IRecipeImageGenerator
{
    /// <summary>
    /// Generates an image for the given recipe and returns its relative URL, or null if generation fails.
    /// </summary>
    Task<string?> GenerateRecipeImageAsync(Guid recipeId, string title, string shortDescription, CancellationToken ct = default);
}
