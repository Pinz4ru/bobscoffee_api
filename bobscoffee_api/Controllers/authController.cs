using Microsoft.AspNetCore.Mvc;
using bobscoffee_api.Data;
using bobscoffee_api.Models;
using bobscoffee_api.Services;

namespace bobscoffee_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AuthController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User input)
        {
            if (_context.Users.Any(u => u.Username == input.Username))
                return BadRequest("Username already exists.");

            var qrPath = QrCodeGenerator.Generate(input.Username, Path.Combine(_env.WebRootPath, "qrcodes"), input.Username);
            input.QrCodePath = qrPath;

            _context.Users.Add(input);
            await _context.SaveChangesAsync();

            return Ok("Registered successfully.");
        }

        [HttpPost("login")]
        public IActionResult Login(User credentials)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == credentials.Username && u.Password == credentials.Password);

            if (user == null) return Unauthorized("Invalid credentials.");

            return Ok(user);
        }

        [HttpPost("scan/{username}")]
        public async Task<IActionResult> Scan(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound("User not found.");

            user.CoffeeCount++;

            if (user.CoffeeCount >= 10)
                user.CoffeeCount = 0; // reset after 10 coffees

            await _context.SaveChangesAsync();

            return Ok(new { user.Username, user.CoffeeCount });
        }

        [HttpGet("getQr/{username}")]
        public IActionResult GetQr(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            var imageBytes = System.IO.File.ReadAllBytes(user.QrCodePath);
            return File(imageBytes, "image/png");
        }
    }
}
