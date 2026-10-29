using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public class InMemoryRecipeService : IRecipeService
{
    private readonly List<Recipe> _recipes =
    [
        new Recipe
        {
            RecipeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Rustic Tomato & Basil Shakshuka",
            ShortDescription = "Poached eggs nestled in a simmered, spiced tomato and bell pepper sauce, finished with fresh garden basil and feta.",
            Instructions = "1. Heat oil in skillet. Sauté onions and peppers. 2. Add crushed tomatoes, cumin, and garlic. 3. Make wells and crack eggs. 4. Cover and simmer until egg whites are set. 5. Top with fresh basil and feta.",
            ImageUrl = "https://images.unsplash.com/photo-1590412200988-a436970781fa?q=80&w=800&auto=format&fit=crop",
            IsPublic = true,
            PrepTimeMinutes = 25,
            Servings = 3,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Ingredients =
            [
                new RecipeIngredient { Name = "Eggs", Quantity = 4, Unit = "pcs" },
                new RecipeIngredient { Name = "Ripe Tomatoes", Quantity = 400, Unit = "g" },
                new RecipeIngredient { Name = "Bell Pepper", Quantity = 1, Unit = "pcs" },
                new RecipeIngredient { Name = "Garlic", Quantity = 3, Unit = "cloves" },
                new RecipeIngredient { Name = "Fresh Basil", Quantity = 15, Unit = "g" },
                new RecipeIngredient { Name = "Feta Cheese", Quantity = 60, Unit = "g", IsOptional = true }
            ]
        },
        new Recipe
        {
            RecipeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Creamy Garlic Parmesan Pasta",
            ShortDescription = "Velvety homemade pasta swirled in an aromatic roasted garlic and grated parmesan sauce with cracked black pepper.",
            Instructions = "1. Boil pasta until al dente. 2. Melt butter in a pan, sauté minced garlic. 3. Stir in cream and bring to a gentle simmer. 4. Fold in parmesan and toss pasta with reserved pasta water.",
            ImageUrl = "https://images.unsplash.com/photo-1638890763825-e20495f6b819?q=80&w=800&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=NHwxMjA3fDA%3D",
            IsPublic = true,
            PrepTimeMinutes = 20,
            Servings = 4,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            Ingredients =
            [
                new RecipeIngredient { Name = "Fettuccine", Quantity = 350, Unit = "g" },
                new RecipeIngredient { Name = "Heavy Cream", Quantity = 200, Unit = "ml" },
                new RecipeIngredient { Name = "Parmesan", Quantity = 90, Unit = "g" },
                new RecipeIngredient { Name = "Garlic", Quantity = 4, Unit = "cloves" },
                new RecipeIngredient { Name = "Butter", Quantity = 30, Unit = "g" },
                new RecipeIngredient { Name = "Parsley", Quantity = 10, Unit = "g", IsOptional = true }
            ]
        },
        new Recipe
        {
            RecipeId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "Avocado, Lime & Grilled Corn Salad",
            ShortDescription = "A refreshing, vibrant salad bursting with smoky charred corn, diced buttery avocados, red onion, and zesty lime dressing.",
            Instructions = "1. Grill or sear corn until lightly charred. 2. Dice avocado and red onion. 3. Whisk olive oil, lime juice, salt, and cumin. 4. Toss all ingredients together gently.",
            ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?q=80&w=800&auto=format&fit=crop",
            IsPublic = true,
            PrepTimeMinutes = 15,
            Servings = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            Ingredients =
            [
                new RecipeIngredient { Name = "Sweet Corn", Quantity = 2, Unit = "ears" },
                new RecipeIngredient { Name = "Avocado", Quantity = 2, Unit = "pcs" },
                new RecipeIngredient { Name = "Red Onion", Quantity = 0.5m, Unit = "pcs" },
                new RecipeIngredient { Name = "Lime", Quantity = 1, Unit = "pcs" },
                new RecipeIngredient { Name = "Cilantro", Quantity = 15, Unit = "g" },
                new RecipeIngredient { Name = "Olive Oil", Quantity = 20, Unit = "ml" }
            ]
        },
        new Recipe
        {
            RecipeId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Title = "Skillet Lemon Herb Chicken Breast",
            ShortDescription = "Crispy golden seared chicken breasts infused with fresh rosemary, thyme, garlic cloves, and freshly squeezed lemon.",
            Instructions = "1. Season chicken breasts with salt and pepper. 2. Sear in a hot skillet with olive oil for 6 mins per side. 3. Add butter, herbs, and garlic. 4. Squeeze fresh lemon juice over top and baste.",
            ImageUrl = "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?q=80&w=800&auto=format&fit=crop",
            IsPublic = true,
            PrepTimeMinutes = 30,
            Servings = 4,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Ingredients =
            [
                new RecipeIngredient { Name = "Chicken Breast", Quantity = 600, Unit = "g" },
                new RecipeIngredient { Name = "Lemon", Quantity = 1, Unit = "pcs" },
                new RecipeIngredient { Name = "Rosemary", Quantity = 2, Unit = "sprigs" },
                new RecipeIngredient { Name = "Thyme", Quantity = 3, Unit = "sprigs" },
                new RecipeIngredient { Name = "Garlic", Quantity = 4, Unit = "cloves" },
                new RecipeIngredient { Name = "Butter", Quantity = 25, Unit = "g" }
            ]
        },
        new Recipe
        {
            RecipeId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Title = "Warm Roasted Veggie & Quinoa Bowl",
            ShortDescription = "Tender roasted sweet potatoes, broccoli florets, and fluffy nutty quinoa drizzled with a creamy tahini-lemon dressing.",
            Instructions = "1. Roast diced sweet potatoes and broccoli at 200°C for 25 mins. 2. Cook quinoa according to package instructions. 3. Assemble bowls with warm quinoa and roasted veggies. 4. Drizzle with tahini.",
            ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?q=80&w=800&auto=format&fit=crop",
            IsPublic = true,
            PrepTimeMinutes = 35,
            Servings = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-12),
            Ingredients =
            [
                new RecipeIngredient { Name = "Quinoa", Quantity = 150, Unit = "g" },
                new RecipeIngredient { Name = "Sweet Potato", Quantity = 1, Unit = "pcs" },
                new RecipeIngredient { Name = "Broccoli", Quantity = 200, Unit = "g" },
                new RecipeIngredient { Name = "Tahini", Quantity = 40, Unit = "g" },
                new RecipeIngredient { Name = "Olive Oil", Quantity = 15, Unit = "ml" }
            ]
        },
        new Recipe
        {
            RecipeId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Title = "Golden French Omelette with Herbs",
            ShortDescription = "Classic velvety French omelette, delicately folded with rich butter, chives, and a melt-in-your-mouth tender center.",
            Instructions = "1. Whisk eggs with a pinch of salt until uniform. 2. Melt butter in a non-stick pan over medium-low heat. 3. Pour in eggs, vigorously stirring and shaking pan. 4. Roll gently and serve immediately.",
            ImageUrl = "https://images.unsplash.com/photo-1525351484163-7529414344d8?q=80&w=800&auto=format&fit=crop",
            IsPublic = true,
            PrepTimeMinutes = 10,
            Servings = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            Ingredients =
            [
                new RecipeIngredient { Name = "Eggs", Quantity = 3, Unit = "pcs" },
                new RecipeIngredient { Name = "Butter", Quantity = 20, Unit = "g" },
                new RecipeIngredient { Name = "Fresh Chives", Quantity = 10, Unit = "g" },
                new RecipeIngredient { Name = "Black Pepper", Quantity = 1, Unit = "pinch", IsOptional = true }
            ]
        }
    ];

    public Task<IReadOnlyList<Recipe>> GetPublicRecipesAsync(string? searchQuery = null)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return Task.FromResult<IReadOnlyList<Recipe>>(_recipes.Where(r => r.IsPublic).ToList());
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

        var filtered = _recipes
            .Where(r => r.IsPublic && (
                terms.Any(term => r.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (r.ShortDescription != null && terms.Any(term => r.ShortDescription.Contains(term, StringComparison.OrdinalIgnoreCase))) ||
                r.Ingredients.Any(i => terms.Any(term => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
            ))
            .ToList();

        return Task.FromResult<IReadOnlyList<Recipe>>(filtered);
    }

    public Task<Recipe?> GetRecipeByIdAsync(Guid recipeId)
    {
        var recipe = _recipes.FirstOrDefault(r => r.RecipeId == recipeId);
        return Task.FromResult(recipe);
    }
}

