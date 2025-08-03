using System.Text.Json.Serialization;

namespace Infrastructure.Dtos;
public class LoginResponseDto
{
    
    public string? Message { get; set;}
    public bool IsAuthenticated { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public string? Token { get; set; }
    [JsonIgnore]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public LoginResponseDto()
    {
        Roles = new List<string>();
    }
}
