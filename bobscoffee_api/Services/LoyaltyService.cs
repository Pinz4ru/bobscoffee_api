using bobscoffee_api.Data;
using bobscoffee_api.Models;

namespace bobscoffee_api.Services
{
    public class LoyaltyService
    {
        private readonly BobsCoffeeContext _context;

        public LoyaltyService(BobsCoffeeContext context)
        {
            _context = context;
        }

        public User? GetUserById(int id) => _context.Users.FirstOrDefault(u => u.Id == id);

        public string GetQrCodePath(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user?.QrCodePath ?? string.Empty;
        }

        public bool AddCoffeeToUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;

            user.CoffeeCount++;
            if (user.CoffeeCount >= 10)
            {
                user.CoffeeCount = 0;
                var stat = _context.LoyaltyStats.FirstOrDefault();
                if (stat == null)
                {
                    stat = new LoyaltyStats
{
    UsedCards = 1
};

                    _context.LoyaltyStats.Add(stat);
                }
                else
                {
                    stat.UsedCards++;
                }
            }

            var transaction = new LoyaltyTransaction
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };
            _context.LoyaltyTransactions.Add(transaction);

            _context.SaveChanges();
            return true;
        }
    }
}
