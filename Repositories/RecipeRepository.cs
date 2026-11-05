using Digital_Fridge.Data;
using Digital_Fridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public RecipeRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<Recipe>> GetPublicRecipesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .Where(r => r.IsPublic)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Recipe>> GetSavedRecipesByContextAsync(Guid? userId, Guid? familyId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.SavedRecipes
            .AsNoTracking()
            .Include(sr => sr.Recipe)
                .ThenInclude(r => r!.Ingredients)
                    .ThenInclude(ri => ri.Product)
            .Where(sr => sr.Recipe != null);

        if (userId.HasValue)
        {
            query = query.Where(sr => sr.UserId == userId.Value);
        }
        else if (familyId.HasValue)
        {
            query = query.Where(sr => sr.FamilyId == familyId.Value);
        }
        else
        {
            return Array.Empty<Recipe>();
        }

        return await query
            .OrderByDescending(sr => sr.SavedAt)
            .Select(sr => sr.Recipe!)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(Guid recipeId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.RecipeId == recipeId);
    }

    public async Task<Recipe> AddAsync(Recipe recipe)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();
        return recipe;
    }

    public async Task UpdateAsync(Recipe recipe)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Recipes.Update(recipe);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid recipeId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var recipe = await context.Recipes.FindAsync(recipeId);
        if (recipe != null)
        {
            context.Recipes.Remove(recipe);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> AnyAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Recipes.AnyAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Recipe> recipes)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.Recipes.AddRangeAsync(recipes);
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<SavedRecipe>> GetSavedRecipesAsync(Guid recipeId, Guid? userId, Guid? familyId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SavedRecipes
            .AsNoTracking()
            .Where(sr => sr.RecipeId == recipeId &&
                ((userId.HasValue && sr.UserId == userId.Value) ||
                 (familyId.HasValue && sr.FamilyId == familyId.Value)))
            .ToListAsync();
    }

    public async Task<bool> ToggleUserBookmarkAsync(Guid recipeId, Guid userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.SavedRecipes.FirstOrDefaultAsync(sr => sr.RecipeId == recipeId && sr.UserId == userId);
        if (existing != null)
        {
            context.SavedRecipes.Remove(existing);
            await context.SaveChangesAsync();
            return false;
        }

        var saved = new SavedRecipe {
            RecipeId = recipeId,
            UserId = userId,
            SavedAt = DateTime.UtcNow
        };
        context.SavedRecipes.Add(saved);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleFamilyBookmarkAsync(Guid recipeId, Guid familyId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.SavedRecipes.FirstOrDefaultAsync(sr => sr.RecipeId == recipeId && sr.FamilyId == familyId);
        if (existing != null)
        {
            context.SavedRecipes.Remove(existing);
            await context.SaveChangesAsync();
            return false;
        }

        var saved = new SavedRecipe {
            RecipeId = recipeId,
            FamilyId = familyId,
            SavedAt = DateTime.UtcNow
        };
        context.SavedRecipes.Add(saved);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MakeRecipePublicAsync(Guid recipeId, Guid authorUserId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.CreatedByUserId == authorUserId);
        if (recipe == null) return false;

        recipe.IsPublic = true;
        await context.SaveChangesAsync();
        return true;
    }
}
