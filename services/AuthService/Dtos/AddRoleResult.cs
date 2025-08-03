namespace Infrastructure.Dtos;

public class AddRoleResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string Role { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}