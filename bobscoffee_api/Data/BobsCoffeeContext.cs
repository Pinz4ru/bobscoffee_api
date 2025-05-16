using Microsoft.EntityFrameworkCore;
using bobscoffee_api.Models;

namespace bobscoffee_api.Data
{
    public class BobsCoffeeContext : DbContext
    {
        public BobsCoffeeContext(DbContextOptions<BobsCoffeeContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
        public DbSet<LoyaltyStats> LoyaltyStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany<LoyaltyTransaction>()
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);
        }
    }
}
