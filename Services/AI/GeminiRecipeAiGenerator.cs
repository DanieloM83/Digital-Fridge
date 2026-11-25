using System.Text;
using System.Text.Json;
using Digital_Fridge.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Digital_Fridge.Services.AI;

public class GeminiRecipeAiGenerator : IRecipeAiGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _modelName;
    private readonly ILogger<GeminiRecipeAiGenerator> _logger;

    public GeminiRecipeAiGenerator(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiRecipeAiGenerator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"]
               ?? configuration["GEMINI_API_KEY"]
               ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        _modelName = configuration["Gemini:RecipeModel"]
                  ?? "gemini-2.5-flash";
    }

    public async Task<IReadOnlyList<Recipe>> GenerateRecipesAsync(
        Guid userId,
        string prompt,
        IReadOnlyList<FridgeItem> availableInventory)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Falling back to contextual fallback generator.");
            return CreateFallbackRecipes(userId, prompt, availableInventory);
        }

        try
        {
            var inventoryList = string.Join(", ", availableInventory
                .Where(i => i.Product != null)
                .Select(i => $"{i.Product!.Name} ({i.Quantity} {i.Unit})"));

            if (string.IsNullOrWhiteSpace(inventoryList))
            {
                inventoryList = "Common kitchen essentials (eggs, pasta, cheese, seasonal vegetables, olive oil)";
            }

            var systemPrompt = $"You are a master culinary chef. Generate exactly 3 distinct, delicious, realistic recipes based on the user's craving: \"{prompt}\" and prioritizing their currently available fridge ingredients: [{inventoryList}]. For each recipe, provide title, short description, instructions with numbered steps, prep time in minutes, and servings.";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

            var requestBody = new {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = systemPrompt }
                        }
                    }
                },
                generationConfig = new {
                    response_mime_type = "application/json",
                    response_schema = new {
                        type = "ARRAY",
                        description = "Exactly 3 generated recipes",
                        items = new {
                            type = "OBJECT",
                            properties = new {
                                title = new { type = "STRING", description = "Recipe title" },
                                shortDescription = new { type = "STRING", description = "1-2 sentence appetizing summary" },
                                instructions = new { type = "STRING", description = "Step by step instructions" },
                                prepTimeMinutes = new { type = "INTEGER", description = "Estimated prep time in minutes" },
                                servings = new { type = "INTEGER", description = "Portion yields" }
                            },
                            required = new[] { "title", "shortDescription", "instructions", "prepTimeMinutes", "servings" }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini Recipe Generation API error ({StatusCode}): {Body}", response.StatusCode, errorBody);
                return CreateFallbackRecipes(userId, prompt, availableInventory);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return CreateFallbackRecipes(userId, prompt, availableInventory);

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(text)) return CreateFallbackRecipes(userId, prompt, availableInventory);

            var parsed = JsonSerializer.Deserialize<List<GeminiRecipeDto>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed == null || parsed.Count == 0) return CreateFallbackRecipes(userId, prompt, availableInventory);

            var fallbackImages = new[]
            {
                "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=800&auto=format&fit=crop",
                "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?q=80&w=800&auto=format&fit=crop",
                "https://images.unsplash.com/photo-1540420773420-3366772f4999?q=80&w=800&auto=format&fit=crop"
            };

            return parsed.Select((dto, index) => new Recipe {
                RecipeId = Guid.NewGuid(),
                CreatedByUserId = userId,
                Title = dto.Title ?? "Chef's Special",
                ShortDescription = dto.ShortDescription ?? "A delightful dish crafted from your fridge items.",
                Instructions = dto.Instructions ?? "1. Prepare ingredients.\n2. Cook with love.\n3. Serve warm.",
                ImageUrl = fallbackImages[index % fallbackImages.Length],
                IsPublic = false,
                PrepTimeMinutes = dto.PrepTimeMinutes > 0 ? dto.PrepTimeMinutes : 20,
                Servings = dto.Servings > 0 ? dto.Servings : 2,
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Gemini Recipe Generation API request timed out (25s). If you are in a network or region where Google AI endpoints are restricted, please verify your internet connection, VPN, or proxy settings.");
            return CreateFallbackRecipes(userId, prompt, availableInventory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate recipes with Gemini. Falling back to default generation.");
            return CreateFallbackRecipes(userId, prompt, availableInventory);
        }
    }

    private static List<Recipe> CreateFallbackRecipes(Guid userId, string prompt, IReadOnlyList<FridgeItem> fridgeItems)
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
                ShortDescription = $"A quick savory dish built around {staple1.ToLowerInvariant()} tailored to: \"{prompt}\".",
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
                ShortDescription = $"A wholesome warm bowl featuring {staple2.ToLowerInvariant()} inspired by: \"{prompt}\".",
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
                ShortDescription = "A vibrant, balanced recipe designed for effortless meal prep with what you have on hand.",
                Instructions = $"1. Lightly sear or warm {staple1.ToLowerInvariant()} in a pan.\n2. Combine in a large serving bowl with mixed greens or available pantry items.\n3. Whisk olive oil, lemon juice, salt, and pepper for dressing.\n4. Drizzle and toss immediately before serving.",
                ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?q=80&w=800&auto=format&fit=crop",
                IsPublic = false,
                PrepTimeMinutes = 15,
                Servings = 2,
                CreatedAt = DateTime.UtcNow
            }
        ];
    }

    private class GeminiRecipeDto
    {
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? Instructions { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int Servings { get; set; }
    }
}
