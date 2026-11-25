using Digital_Fridge.Models;
using Digital_Fridge.Repositories;
using Digital_Fridge.Services;

namespace Digital_Fridge.Tests.Unit;

[Trait("Category", "Unit")]
public class RecipeServiceTests
{
    [Fact]
    public async Task GetPublicRecipesAsync_WithBlankQuery_ReturnsAllRecipes()
    {
        var repo = new FakeRecipeRepository(SampleRecipes());
        var service = new RecipeService(repo);

        var result = await service.GetPublicRecipesAsync("   ");

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetPublicRecipesAsync_FiltersByTitleDescriptionAndIngredientName()
    {
        var repo = new FakeRecipeRepository(SampleRecipes());
        var service = new RecipeService(repo);

        var byTitle = await service.GetPublicRecipesAsync("borscht");
        var byDesc = await service.GetPublicRecipesAsync("creamy");
        var byIngredient = await service.GetPublicRecipesAsync("spinach");

        Assert.Single(byTitle);
        Assert.Equal("Classic Borscht", byTitle[0].Title);

        Assert.Single(byDesc);
        Assert.Equal("Garlic Pasta", byDesc[0].Title);

        Assert.Single(byIngredient);
        Assert.Equal("Green Omelette", byIngredient[0].Title);
    }

    [Fact]
    public async Task GetPublicRecipesAsync_CommaSeparatedTerms_MatchesAnyTerm()
    {
        var repo = new FakeRecipeRepository(SampleRecipes());
        var service = new RecipeService(repo);

        var result = await service.GetPublicRecipesAsync("borscht, spinach");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ToggleBookmarkAsync_FamilyTargetWithoutFamilyId_ReturnsFalse()
    {
        var repo = new FakeRecipeRepository([]);
        var service = new RecipeService(repo);

        var toggled = await service.ToggleBookmarkAsync(Guid.NewGuid(), Guid.NewGuid(), null, "family");

        Assert.False(toggled);
        Assert.False(repo.FamilyBookmarkCalled);
    }

    [Fact]
    public async Task ToggleBookmarkAsync_PersonalTarget_DelegatesToUserBookmark()
    {
        var repo = new FakeRecipeRepository([]);
        var service = new RecipeService(repo);

        var toggled = await service.ToggleBookmarkAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "personal");

        Assert.True(toggled);
        Assert.True(repo.UserBookmarkCalled);
        Assert.False(repo.FamilyBookmarkCalled);
    }

    private static List<Recipe> SampleRecipes() =>
    [
        new Recipe {
            RecipeId = Guid.NewGuid(),
            Title = "Classic Borscht",
            ShortDescription = "Hearty beet soup",
            Instructions = "Simmer",
            IsPublic = true
        },
        new Recipe {
            RecipeId = Guid.NewGuid(),
            Title = "Garlic Pasta",
            ShortDescription = "Rich and creamy dinner",
            Instructions = "Boil",
            IsPublic = true
        },
        new Recipe {
            RecipeId = Guid.NewGuid(),
            Title = "Green Omelette",
            ShortDescription = "Quick breakfast",
            Instructions = "Whisk",
            IsPublic = true,
            Ingredients = [
                new RecipeIngredient {
                    Product = new Product { Name = "Fresh Spinach" }
                }
            ]
        }
    ];

    private sealed class FakeRecipeRepository(IReadOnlyList<Recipe> publicRecipes) : IRecipeRepository
    {
        public bool UserBookmarkCalled { get; private set; }
        public bool FamilyBookmarkCalled { get; private set; }

        public Task<IReadOnlyList<Recipe>> GetPublicRecipesAsync() => Task.FromResult(publicRecipes);
        public Task<IReadOnlyList<Recipe>> GetSavedRecipesByContextAsync(Guid? userId, Guid? familyId) => Task.FromResult(publicRecipes);
        public Task<Recipe?> GetByIdAsync(Guid recipeId) => Task.FromResult<Recipe?>(null);
        public Task<Recipe> AddAsync(Recipe recipe) => Task.FromResult(recipe);
        public Task UpdateAsync(Recipe recipe) => Task.CompletedTask;
        public Task DeleteAsync(Guid recipeId) => Task.CompletedTask;
        public Task<bool> AnyAsync() => Task.FromResult(publicRecipes.Count > 0);
        public Task AddRangeAsync(IEnumerable<Recipe> recipes) => Task.CompletedTask;
        public Task<IReadOnlyList<SavedRecipe>> GetSavedRecipesAsync(Guid recipeId, Guid? userId, Guid? familyId) => Task.FromResult<IReadOnlyList<SavedRecipe>>([]);

        public Task<bool> ToggleUserBookmarkAsync(Guid recipeId, Guid userId)
        {
            UserBookmarkCalled = true;
            return Task.FromResult(true);
        }

        public Task<bool> ToggleFamilyBookmarkAsync(Guid recipeId, Guid familyId)
        {
            FamilyBookmarkCalled = true;
            return Task.FromResult(true);
        }

        public Task<bool> MakeRecipePublicAsync(Guid recipeId, Guid authorUserId) => Task.FromResult(true);
    }
}
