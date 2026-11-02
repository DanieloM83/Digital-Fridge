using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class Recipe
{
    [Key]
    public Guid RecipeId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    [Required]
    public string Instructions { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    public bool IsPublic { get; set; } = false;

    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int PrepTimeMinutes { get; set; }

    public int Servings { get; set; }

    public List<RecipeIngredient> Ingredients { get; set; } = [];
    public List<SavedRecipe> SavedBy { get; set; } = [];
}
