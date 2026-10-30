namespace Digital_Fridge.Models;

public class RecipeIngredient
{
    public Guid RecipeId { get; set; }
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}

