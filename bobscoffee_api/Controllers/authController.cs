using Microsoft.AspNetCore.Mvc;
using bobscoffee_api.Services;
using bobscoffee_api.DTOs;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using bobscoffee_api.Models;

namespace bobscoffee_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IWebHostEnvironment env,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _env = env;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { Message = "All fields are required" });
                }

                if (request.Password.Length < 8)
                {
                    return BadRequest(new { Message = "Password must be at least 8 characters" });
                }

                // Check if user exists
                if (await _authService.UserExistsAsync(request.Username))
                {
                    return Conflict(new { Message = "Username already exists" });
                }

                // Create user
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email
                };

                // Register user (password hashing happens in service)
                var qrCodeDirectory = Path.Combine(_env.WebRootPath, "qrcodes");
                var registeredUser = await _authService.RegisterAsync(user, request.Password, qrCodeDirectory);

                if (registeredUser == null)
                {
                    return StatusCode(500, new { Message = "Registration failed" });
                }

                _logger.LogInformation($"New user registered: {registeredUser.Username}");

                return Ok(new AuthResponse
                {
                    Username = registeredUser.Username,
                    Email = registeredUser.Email,
                    CoffeeCount = registeredUser.CoffeeCount,
                    QrCodeUrl = $"/api/auth/qr/{registeredUser.Username}",
                    Message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { Message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _authService.LoginAsync(request.Username, request.Password);

                if (user == null)
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // Ensure critical fields exist
                if (string.IsNullOrEmpty(user.Id.ToString()) || string.IsNullOrEmpty(user.Roles))
                    throw new Exception("User data incomplete");

                return Ok(new AuthResponse
                {
                    Username = user.Username,
                    Email = user.Email,
                    CoffeeCount = user.CoffeeCount,
                    QrCodeUrl = $"/api/auth/qr/{user.Username}",
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }

        [Authorize]
        [HttpPost("scan/{username}")]
        public async Task<IActionResult> ScanCoffee(string username)
        {
            try
            {
                var user = await _authService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                user.CoffeeCount++;
                bool isFreeCoffee = user.CoffeeCount >= 10;

                if (isFreeCoffee)
                {
                    user.CoffeeCount = 0;
                }

                var success = await _authService.UpdateUserAsync(user);
                if (!success)
                {
                    return StatusCode(500, new { Message = "Failed to update coffee count" });
                }

                _logger.LogInformation($"Coffee scanned for {user.Username}. Total: {user.CoffeeCount}");

                return Ok(new CoffeeScanResponse
                {
                    Username = user.Username,
                    CoffeeCount = user.CoffeeCount,
                    IsFreeCoffee = isFreeCoffee,
                    Message = isFreeCoffee ? "Congratulations! This coffee is free!" : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during coffee scan");
                return StatusCode(500, new { Message = "An error occurred while processing your coffee" });
            }
        }

        [HttpGet("qr/{username}")]
        public async Task<IActionResult> GetQrCode(string username)
        {
            try
            {
                var user = await _authService.GetUserByUsernameAsync(username);
                if (user == null || !System.IO.File.Exists(user.QrCodePath))
                {
                    return NotFound(new { Message = "QR code not found" });
                }

                var imageBytes = await System.IO.File.ReadAllBytesAsync(user.QrCodePath);
                return File(imageBytes, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving QR code");
                return StatusCode(500, new { Message = "An error occurred while retrieving QR code" });
            }
        }
    }
}