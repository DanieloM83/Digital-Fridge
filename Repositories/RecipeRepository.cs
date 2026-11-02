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
            .Where(r => r.IsPublic)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(Guid recipeId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
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
}

