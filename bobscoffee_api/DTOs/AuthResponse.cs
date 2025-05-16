namespace bobscoffee_api.DTOs
{
    public class AuthResponse
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CoffeeCount { get; set; }
        public string QrCodeUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}