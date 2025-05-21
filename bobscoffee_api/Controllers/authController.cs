using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using bobscoffee_api.DTOs;
using bobscoffee_api.Services;

namespace bobscoffee_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            IWebHostEnvironment env,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _authService = authService;
            _env = env;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validation
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

                // Create user object
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    CoffeeCount = 0
                };

                // Ensure qrcodes directory exists
                var qrCodeDirectory = Path.Combine(_env.WebRootPath, "qrcodes");
                Directory.CreateDirectory(qrCodeDirectory);

                // Register user
                var registeredUser = await _authService.RegisterAsync(user, request.Password, qrCodeDirectory);

                if (registeredUser == null)
                {
                    return Conflict(new { Message = "Username already exists" });
                }

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
                _logger.LogError(ex, "Registration error");
                return StatusCode(500, new { Message = "Registration failed. Please try again." });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Verify session is available
                if (HttpContext.Session == null)
                {
                    throw new InvalidOperationException("Session is not available");
                }

                var user = await _authService.LoginAsync(request.Username, request.Password);
                if (user == null)
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // Store user information in the session
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Roles", user.Roles);

                return Ok(new AuthResponse
                {
                    Username = user.Username,
                    Email = user.Email,
                    Roles = user.Roles.Split(',').ToList(),
                    CoffeeCount = user.CoffeeCount,
                    QrCodeUrl = $"/api/auth/qr/{user.Username}",
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Username}", request.Username);
                return StatusCode(500, new
                {
                    Message = "Login failed",
                    Detail = ex.Message
                });
            }
        }

        [Authorize(Roles = "Admin,Barista")]
        [HttpPost("scan/{username}")]
        public async Task<IActionResult> ScanCoffee(string username)
        {
            try
            {
                // Get current barista/admin info
                var performerName = User.FindFirst(ClaimTypes.Name)?.Value;

                // Get user
                var user = await _authService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Update coffee count
                user.CoffeeCount++;
                bool isFreeCoffee = user.CoffeeCount >= 10;

                if (isFreeCoffee)
                {
                    user.CoffeeCount = 0; // Reset counter
                    _logger.LogInformation($"Free coffee awarded to {user.Username} by {performerName}");
                }

                // Save changes
                var success = await _authService.UpdateUserAsync(user);
                if (!success)
                {
                    return StatusCode(500, new { Message = "Failed to update coffee count" });
                }

                _logger.LogInformation($"{performerName} scanned coffee for {user.Username}. Total: {user.CoffeeCount}");

                return Ok(new CoffeeScanResponse
                {
                    Username = user.Username,
                    CoffeeCount = user.CoffeeCount,
                    IsFreeCoffee = isFreeCoffee,
                    Message = isFreeCoffee ? "Free coffee awarded!" : "Coffee added successfully"
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

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentRequest request)
        {
            try
            {
                var adminUsername = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = await _authService.GetUserByUsernameAsync(request.Username);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Prevent modifying other admins unless super-admin
                if (user.Roles.Contains("Admin") && !User.IsInRole("SuperAdmin"))
                {
                    return Forbid();
                }

                var roles = user.Roles.Split(',').ToList();

                if (request.Assign)
                {
                    if (!roles.Contains(request.Role))
                    {
                        roles.Add(request.Role);
                    }
                }
                else
                {
                    roles.Remove(request.Role);
                }

                user.Roles = string.Join(",", roles.Distinct());
                await _authService.UpdateUserAsync(user);

                _logger.LogInformation($"User {request.Username} roles updated by {adminUsername}. New roles: {user.Roles}");

                return Ok(new
                {
                    user.Username,
                    Roles = user.Roles.Split(',')
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role");
                return StatusCode(500, new { Message = "An error occurred while assigning role" });
            }
        }
    }
}