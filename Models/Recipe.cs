namespace Digital_Fridge.Models;

public class Recipe
{
    public Guid RecipeId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsPublic { get; set; } = true;
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Added UI/discovery metadata properties
    public int PrepTimeMinutes { get; set; }
    public int Servings { get; set; }
    public List<RecipeIngredient> Ingredients { get; set; } = [];
}

