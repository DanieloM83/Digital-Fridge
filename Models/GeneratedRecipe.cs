using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class GeneratedRecipe
{
    [Key]
    public Guid GeneratedRecipeId { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }
    public GeneratedRecipeSession? Session { get; set; }

    public Guid RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
}
