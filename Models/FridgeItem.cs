using System.ComponentModel.DataAnnotations;
using Digital_Fridge.Models.Enums;

namespace Digital_Fridge.Models;

public class FridgeItem
{
    [Key]
    public Guid FridgeItemId { get; set; } = Guid.NewGuid();

    public Guid FamilyId { get; set; }
    public Family? Family { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal Quantity { get; set; }

    public UnitEnum Unit { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public EntryMethod EntryMethod { get; set; } = EntryMethod.Manual;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

