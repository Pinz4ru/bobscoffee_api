// Models/User.cs
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Roles { get; set; } = "Customer"; // Comma-separated roles
    public int CoffeeCount { get; set; }
    public string QrCodePath { get; set; } = string.Empty;

    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsBarista => Roles.Contains("Barista");
}