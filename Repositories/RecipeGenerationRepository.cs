using Digital_Fridge.Data;
using Digital_Fridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Repositories;

public class RecipeGenerationRepository : IRecipeGenerationRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public RecipeGenerationRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<GeneratedRecipeSession?> GetSessionByUserIdAsync(Guid userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.GeneratedRecipeSessions
            .AsNoTracking()
            .Include(s => s.Items)
                .ThenInclude(i => i.Recipe)
                    .ThenInclude(r => r!.Ingredients)
                        .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<GeneratedRecipeSession> SaveSessionAsync(Guid userId, string prompt, IReadOnlyList<Recipe> recipes)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Fetch existing session with items and recipe links
        var existingSession = await context.GeneratedRecipeSessions
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (existingSession != null)
        {
            var oldRecipeIds = existingSession.Items.Select(i => i.RecipeId).Distinct().ToList();

            // Remove the session items and the session record itself
            context.GeneratedRecipes.RemoveRange(existingSession.Items);
            context.GeneratedRecipeSessions.Remove(existingSession);
            await context.SaveChangesAsync();

            // 2. Cascade delete old recipes IF they are not saved in SavedRecipes
            foreach (var recipeId in oldRecipeIds)
            {
                var isSaved = await context.SavedRecipes.AnyAsync(sr => sr.RecipeId == recipeId);
                if (!isSaved)
                {
                    var oldRecipe = await context.Recipes.FindAsync(recipeId);
                    if (oldRecipe != null)
                    {
                        context.Recipes.Remove(oldRecipe);
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        // 3. Persist the newly generated recipes
        foreach (var recipe in recipes)
        {
            if (recipe.RecipeId == Guid.Empty)
            {
                recipe.RecipeId = Guid.NewGuid();
            }
            await context.Recipes.AddAsync(recipe);
        }
        await context.SaveChangesAsync();

        // 4. Create new GeneratedRecipeSession
        var newSession = new GeneratedRecipeSession {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            Prompt = prompt.Trim(),
            CreatedAt = DateTime.UtcNow,
            Items = recipes.Select(r => new GeneratedRecipe {
                GeneratedRecipeId = Guid.NewGuid(),
                RecipeId = r.RecipeId
            }).ToList()
        };

        await context.GeneratedRecipeSessions.AddAsync(newSession);
        await context.SaveChangesAsync();

        return newSession;
    }
}
