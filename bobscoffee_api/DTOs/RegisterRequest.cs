namespace bobscoffee_api.DTOs
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // plain password from client, to be hashed
    }

}
