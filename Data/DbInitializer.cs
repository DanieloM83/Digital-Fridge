using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Data;

public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var context = await contextFactory.CreateDbContextAsync();

        // Apply all pending migrations and create __EFMigrationsHistory
        await context.Database.MigrateAsync();

        if (!await context.ProductCategories.AnyAsync())
        {
            await SeedCategoriesAndProductsAsync(context);
        }

        if (!await context.Recipes.AnyAsync())
        {
            await SeedInitialRecipesAsync(context);
        }
    }

    private static async Task SeedCategoriesAndProductsAsync(AppDbContext context)
    {
        var produce = new ProductCategory { Name = "Produce", IconUrl = "bi-flower1" };
        var dairy = new ProductCategory { Name = "Dairy & Eggs", IconUrl = "bi-egg" };
        var meat = new ProductCategory { Name = "Meat & Poultry", IconUrl = "bi-cup-straw" };
        var pantry = new ProductCategory { Name = "Pantry & Grains", IconUrl = "bi-basket" };
        var bakery = new ProductCategory { Name = "Bakery", IconUrl = "bi-bag" };
        var condiments = new ProductCategory { Name = "Condiments & Spices", IconUrl = "bi-droplet" };
        var beverages = new ProductCategory { Name = "Beverages", IconUrl = "bi-cup-hot" };

        await context.ProductCategories.AddRangeAsync(produce, dairy, meat, pantry, bakery, condiments, beverages);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            // Dairy & Eggs
            new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000001"), Name = "Eggs", CategoryId = dairy.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000002"), Name = "Feta Cheese", CategoryId = dairy.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Processed },
            new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000003"), Name = "Heavy Cream", CategoryId = dairy.CategoryId, DefaultUnit = UnitEnum.Milliliters, ProcessingLevel = ProcessingLevel.Processed },
            new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000004"), Name = "Parmesan", CategoryId = dairy.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Processed },
            new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000005"), Name = "Butter", CategoryId = dairy.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Processed },

            // Produce
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000001"), Name = "Ripe Tomatoes", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000002"), Name = "Bell Pepper", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000003"), Name = "Garlic", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Cloves, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000004"), Name = "Fresh Basil", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000005"), Name = "Parsley", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000006"), Name = "Sweet Corn", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000007"), Name = "Avocado", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000008"), Name = "Red Onion", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000009"), Name = "Lime", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000010"), Name = "Cilantro", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000011"), Name = "Lemon", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000012"), Name = "Rosemary", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Sprigs, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000013"), Name = "Thyme", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Sprigs, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000014"), Name = "Sweet Potato", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Pieces, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000015"), Name = "Broccoli", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000016"), Name = "Fresh Chives", CategoryId = produce.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },

            // Meat & Poultry
            new() { ProductId = Guid.Parse("c0000000-0000-0000-0000-000000000001"), Name = "Chicken Breast", CategoryId = meat.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },

            // Pantry & Grains
            new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000001"), Name = "Fettuccine", CategoryId = pantry.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Processed },
            new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000002"), Name = "Olive Oil", CategoryId = condiments.CategoryId, DefaultUnit = UnitEnum.Milliliters, ProcessingLevel = ProcessingLevel.Processed },
            new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000003"), Name = "Quinoa", CategoryId = pantry.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Raw },
            new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000004"), Name = "Tahini", CategoryId = condiments.CategoryId, DefaultUnit = UnitEnum.Grams, ProcessingLevel = ProcessingLevel.Processed },
            new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000005"), Name = "Black Pepper", CategoryId = condiments.CategoryId, DefaultUnit = UnitEnum.Pinch, ProcessingLevel = ProcessingLevel.Raw }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInitialRecipesAsync(AppDbContext context)
    {
        var recipes = new List<Recipe>
        {
            new()
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
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000001"), Quantity = 4, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000001"), Quantity = 400, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000002"), Quantity = 1, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000003"), Quantity = 3, Unit = UnitEnum.Cloves },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000004"), Quantity = 15, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000002"), Quantity = 60, Unit = UnitEnum.Grams, IsOptional = true }
                ]
            },
            new()
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
                    new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000001"), Quantity = 350, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000003"), Quantity = 200, Unit = UnitEnum.Milliliters },
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000004"), Quantity = 90, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000003"), Quantity = 4, Unit = UnitEnum.Cloves },
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000005"), Quantity = 30, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000005"), Quantity = 10, Unit = UnitEnum.Grams, IsOptional = true }
                ]
            },
            new()
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
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000006"), Quantity = 2, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000007"), Quantity = 2, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000008"), Quantity = 0.5m, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000009"), Quantity = 1, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000010"), Quantity = 15, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000002"), Quantity = 20, Unit = UnitEnum.Milliliters }
                ]
            },
            new()
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
                    new() { ProductId = Guid.Parse("c0000000-0000-0000-0000-000000000001"), Quantity = 600, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000011"), Quantity = 1, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000012"), Quantity = 2, Unit = UnitEnum.Sprigs },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000013"), Quantity = 3, Unit = UnitEnum.Sprigs },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000003"), Quantity = 4, Unit = UnitEnum.Cloves },
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000005"), Quantity = 25, Unit = UnitEnum.Grams }
                ]
            },
            new()
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
                    new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000003"), Quantity = 150, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000014"), Quantity = 1, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000015"), Quantity = 200, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000004"), Quantity = 40, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000002"), Quantity = 15, Unit = UnitEnum.Milliliters }
                ]
            },
            new()
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
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000001"), Quantity = 3, Unit = UnitEnum.Pieces },
                    new() { ProductId = Guid.Parse("a0000000-0000-0000-0000-000000000005"), Quantity = 20, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("b0000000-0000-0000-0000-000000000016"), Quantity = 10, Unit = UnitEnum.Grams },
                    new() { ProductId = Guid.Parse("d0000000-0000-0000-0000-000000000005"), Quantity = 1, Unit = UnitEnum.Pinch, IsOptional = true }
                ]
            }
        };

        await context.Recipes.AddRangeAsync(recipes);
        await context.SaveChangesAsync();
    }
}
