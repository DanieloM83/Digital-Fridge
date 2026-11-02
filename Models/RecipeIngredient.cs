using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Digital_Fridge.Models;

public class RecipeIngredient
{
    public Guid RecipeId { get; set; }

    [JsonIgnore]
    public Recipe? Recipe { get; set; }

    public Guid ProductId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    public bool IsOptional { get; set; }
}
