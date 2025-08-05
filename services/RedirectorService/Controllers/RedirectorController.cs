using Microsoft.AspNetCore.Mvc;

namespace RedirectorService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RedirectorController : ControllerBase
{
    public RedirectorController()
    {
    }
    [HttpGet("redirect")]
    public IActionResult RedirectToUrl([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL cannot be empty.");
        }
        
        return Redirect(url);
    }
    
}