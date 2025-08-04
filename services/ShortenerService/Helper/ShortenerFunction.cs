namespace ShortenerService.Helper;

public static class ShortenerFunction
{
    public static string GenerateShortenedUrl(string originalUrl)
    {
        if (string.IsNullOrEmpty(originalUrl))
        {
            throw new ArgumentException("Original URL cannot be null or empty.", nameof(originalUrl));
        }
        var hash = Guid.NewGuid().ToString("N").Substring(0, 8);
        return $"https://short.ly/{hash}";
    }
}