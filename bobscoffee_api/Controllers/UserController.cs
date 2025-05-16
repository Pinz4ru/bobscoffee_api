using Microsoft.AspNetCore.Mvc;
using bobscoffee_api.Models;
using bobscoffee_api.Services;

namespace bobscoffee_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly LoyaltyService _loyaltyService;

        public UserController(LoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _loyaltyService.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("{id}/qrcode")]
        public IActionResult GetQrCode(int id)
        {
            var path = _loyaltyService.GetQrCodePath(id);
            if (System.IO.File.Exists(path))
            {
                var imageBytes = System.IO.File.ReadAllBytes(path);
                return File(imageBytes, "image/png");
            }
            return NotFound("QR code not found");
        }
    }
}
