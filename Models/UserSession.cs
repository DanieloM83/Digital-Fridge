using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class UserSession
{
    [Key]
    [MaxLength(64)]
    public required string SessionId { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; } = false;
}
