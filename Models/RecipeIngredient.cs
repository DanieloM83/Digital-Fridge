using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Digital_Fridge.Models.Enums;

namespace Digital_Fridge.Models;

public class RecipeIngredient
{
    public Guid RecipeId { get; set; }

    [JsonIgnore]
    public Recipe? Recipe { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal Quantity { get; set; }

    public UnitEnum Unit { get; set; }

    public bool IsOptional { get; set; } = false;

    // Helper to maintain backward compatibility with UI components reading ingredient.Name
    [NotMapped]
    public string Name => Product?.Name ?? string.Empty;
}
