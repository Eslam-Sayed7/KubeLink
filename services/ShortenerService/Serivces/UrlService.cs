using ShortenerService.Base;
using ShortenerService.Entities;
using ShortenerService.Helper;

namespace ShortenerService.Serivces;

public class UrlService : IUrlSerivce
{
    private readonly IUnitOfWork _unitOfWork;
    
    public UrlService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public string ShortenUrl(string url , string userId )
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }
        
        var shortUrl = ShortenerFunction.GenerateShortenedUrl(url); 
        
        var urlEntity = new URL
        {
            OriginalUrl = url,
            UserId = userId,
            ShortenedUrl = shortUrl,
            CreatedAt = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            AccessCount = 0
        };

        _unitOfWork.Repository<URL>().AddAsync(urlEntity);
        _unitOfWork.CompleteAsync();

        return shortUrl;
    }

    public async Task<string> ExpandUrl(string shortUrl)
    {
        if (string.IsNullOrWhiteSpace(shortUrl))
        {
            throw new ArgumentException("Short URL cannot be null or empty.", nameof(shortUrl));
        }

        var urlEntity = await _unitOfWork.Repository<URL>()
            .FindAsync(u => u.ShortenedUrl == shortUrl);
        
        if (urlEntity == null || !urlEntity.Any())
        {
            throw new KeyNotFoundException("Short URL not found.");
        }
        var url = urlEntity.FirstOrDefault();
        
        url.AccessCount++;

        _unitOfWork.Repository<URL>().UpdateEntity(url);
        _unitOfWork.CompleteAsync();

        return url.OriginalUrl;
    }
}