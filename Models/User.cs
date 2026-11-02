using System.ComponentModel.DataAnnotations;
using Digital_Fridge.Models.Enums;

namespace Digital_Fridge.Models;

public class User
{
    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(256)]
    public string GoogleId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? FullName { get; set; }

    [MaxLength(1000)]
    public string? AvatarUrl { get; set; }

    public UserRole Role { get; set; } = UserRole.FamilyHead;

    public Guid FamilyId { get; set; }
    public Family? Family { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<UserSession> Sessions { get; set; } = [];
    public List<SavedRecipe> SavedRecipes { get; set; } = [];
    public List<Recipe> CreatedRecipes { get; set; } = [];
}

