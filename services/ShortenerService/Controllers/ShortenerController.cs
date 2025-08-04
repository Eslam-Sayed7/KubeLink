using Microsoft.AspNetCore.Mvc;
using ShortenerService.Helper;

namespace ShortenerService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShortenerController : ControllerBase
{
    [HttpGet("shorten")]
    public IActionResult ShortenUrl([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL cannot be empty.");
        }
        var shortenedUrl = ShortenerFunction.GenerateShortenedUrl(url);
        return Ok(new { ShortenedUrl = shortenedUrl });
    }
}