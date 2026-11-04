using System.ComponentModel.DataAnnotations;
using Digital_Fridge.Models.Enums;

namespace Digital_Fridge.Models;

public class Product
{
    [Key]
    public Guid ProductId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    public ProcessingLevel? ProcessingLevel { get; set; }

    public UnitEnum DefaultUnit { get; set; } = UnitEnum.Grams;

    [MaxLength(100)]
    public string? Barcode { get; set; }

    public List<FridgeItem> FridgeItems { get; set; } = [];
    public List<RecipeIngredient> RecipeIngredients { get; set; } = [];
}

