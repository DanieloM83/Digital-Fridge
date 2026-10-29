namespace Digital_Fridge.Models;

public class UserSession
{
    public required string SessionId { get; set; }
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? PictureUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
}

