using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class ProductCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? IconUrl { get; set; }

    public List<Product> Products { get; set; } = [];
}

