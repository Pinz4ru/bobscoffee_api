namespace bobscoffee_api.DTOs
{
    public class RoleAssignmentRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Assign { get; set; } 
    }
}
