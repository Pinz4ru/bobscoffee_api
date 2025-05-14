namespace bobscoffee_api.Models
{ 
    public class LoyaltyTransaction
    {
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ActionType { get; set; } = null!;
    public int PointsChanged { get; set; }
    public DateTime Timestamp { get; set; }

    public User User { get; set; } = null!;
    }
}