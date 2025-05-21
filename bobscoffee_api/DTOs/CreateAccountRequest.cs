using System.ComponentModel.DataAnnotations;

namespace bobscoffee_api.DTOs
{
    public class CreateAccountRequest
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
    }
}
