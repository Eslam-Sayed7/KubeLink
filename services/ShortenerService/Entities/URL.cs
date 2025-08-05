namespace ShortenerService.Entities;

public class URL
{
    public int Id { get; set; }
    public string UserId { get; set; } 
    public string OriginalUrl { get; set; }
    public string ShortenedUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AccessCount { get; set; }
    public DateTime? ExpirationDate { get; set; }
}