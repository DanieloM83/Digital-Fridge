using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class Family
{
    [Key]
    public Guid FamilyId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<User> Members { get; set; } = [];
    public List<FridgeItem> FridgeItems { get; set; } = [];
    public List<SavedRecipe> SavedRecipes { get; set; } = [];
}

