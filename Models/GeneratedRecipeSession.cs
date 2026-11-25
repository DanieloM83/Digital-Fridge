using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class GeneratedRecipeSession
{
    [Key]
    public Guid SessionId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Prompt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<GeneratedRecipe> Items { get; set; } = [];
}
