using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Entities;
public class AppUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Token { get; set; }
    public List<RefreshToken>? RefreshTokens { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}

    
