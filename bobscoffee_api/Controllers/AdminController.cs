using bobscoffee_api.DTOs;
using bobscoffee_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin")]
//[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAuthService authService, ILogger<AdminController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("Admin controller reachable");
    [HttpPost("accounts/test")]
    public async Task<IActionResult> TestCreate()
    {
        try
        {
            var user = new User
            {
                Username = "admin_test",
                Email = "test@example.com",
                Roles = "Admin"
            };

            var created = await _authService.CreateUserAsync(user, "Test123!");
            return Ok(created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    // Account Management
    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Roles = request.Role,
                CoffeeCount = 0
            };

            var createdUser = await _authService.CreateUserAsync(user, request.Password);
            return Ok(createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("accounts/{username}")]
    public async Task<IActionResult> UpdateAccount(string username, [FromBody] UpdateAccountRequest request)
    {
        try
        {
            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            user.Email = request.Email ?? user.Email;
            user.Roles = request.Role ?? user.Roles;

            await _authService.UpdateUserAsync(user);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("accounts/{username}")]
    public async Task<IActionResult> DeleteAccount(string username)
    {
        try
        {
            var success = await _authService.DeleteUserAsync(username);
            return success ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account");
            return BadRequest(new { Message = ex.Message });
        }
    }

    // Coffee Management
    [HttpPost("scan/{username}")]
    public async Task<IActionResult> AdminScan(string username, [FromQuery] int amount = 1)
    {
        try
        {
            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            user.CoffeeCount += amount;
            bool isFreeCoffee = user.CoffeeCount >= 10;

            if (isFreeCoffee)
            {
                user.CoffeeCount = 0;
            }

            await _authService.UpdateUserAsync(user);

            return Ok(new CoffeeScanResponse
            {
                Username = user.Username,
                CoffeeCount = user.CoffeeCount,
                IsFreeCoffee = isFreeCoffee
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in admin scan");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPatch("loyalty/{username}/reset")]
    public async Task<IActionResult> ResetCoffeeCount(string username)
    {
        try
        {
            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            user.CoffeeCount = 0;
            await _authService.UpdateUserAsync(user);

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting coffee count");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    [HttpPatch("loyalty/{username}/remove")]
    public async Task<IActionResult> RemoveOneCoffee(string username)
    {
        try
        {
            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            user.CoffeeCount = Math.Max(0, user.CoffeeCount - 1);
            await _authService.UpdateUserAsync(user);

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing coffee");
            return StatusCode(500, new { Message = ex.Message });
        }
    }

    // Get all users with their loyalty status
    [HttpGet("loyalty")]
    public async Task<IActionResult> GetAllLoyaltyCards()
    {
        try
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users.Select(u => new
            {
                u.Username,
                u.Email,
                u.CoffeeCount,
                ProgressToFreeCoffee = u.CoffeeCount % 10,
                u.Roles
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty cards");
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}