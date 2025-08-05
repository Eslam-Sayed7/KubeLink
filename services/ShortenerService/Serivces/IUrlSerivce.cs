namespace ShortenerService.Serivces;

public interface IUrlSerivce
{
    /// <summary>
    ///     Shortens the given URL.
    /// </summary>
    /// <param name="url">The URL to shorten.</param>
    /// <returns>A shortened version of the URL.</returns>
    string ShortenUrl(string url , string userId);

    /// <summary>
    ///     Expands the shortened URL back to its original form.
    /// </summary>
    /// <param name="shortUrl">The shortened URL to expand.</param>
    /// <returns>The original URL.</returns>
    Task<string> ExpandUrl(string shortUrl);
    
}