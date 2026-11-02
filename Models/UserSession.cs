using System.ComponentModel.DataAnnotations;

namespace Digital_Fridge.Models;

public class UserSession
{
    [Key]
    [MaxLength(64)]
    public required string SessionId { get; set; }

    [Required]
    [MaxLength(128)]
    public required string UserId { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(256)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? PictureUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }
}
