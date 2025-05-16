using Microsoft.AspNetCore.Mvc;
using bobscoffee_api.Services;

namespace bobscoffee_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly LoyaltyService _loyaltyService;

        public AdminController(LoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpPost("{id}/addcoffee")]
        public IActionResult AddCoffee(int id)
        {
            var result = _loyaltyService.AddCoffeeToUser(id);
            return result ? Ok("Coffee added.") : NotFound("User not found.");
        }
    }
}
