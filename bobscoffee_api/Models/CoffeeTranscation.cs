namespace bobscoffee_api.Models
{
    public class CoffeeTransaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty; // Foreign key to User
        public User User { get; set; } = null!; // Navigation property
        public int Amount { get; set; }
        public bool IsFreeCoffee { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? BaristaId { get; set; } // Who performed the transaction
    }
}
