using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class SavedRecipe
{
    [Key]
    public Guid SavedRecipeId { get; set; } = Guid.NewGuid();

    public Guid RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? FamilyId { get; set; }
    public Family? Family { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

