using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

[ApiController]
[Route("api/[controller]")]
public class LoyaltyController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public LoyaltyController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("loyaltycard")]
    public IActionResult GetLoyaltyCard()
    {
        var imagePath = Path.Combine(_env.WebRootPath, "loyalty_card.jpeg");

        if (!System.IO.File.Exists(imagePath))
        {
            return NotFound("Image not found.");
        }

        var imageBytes = System.IO.File.ReadAllBytes(imagePath);
        return File(imageBytes, "image/jpeg");
    }
}
