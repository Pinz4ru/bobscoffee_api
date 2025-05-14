using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using bobscoffee_api.Models;

public class BobsCoffeeContext : DbContext
{
    public BobsCoffeeContext(DbContextOptions<BobsCoffeeContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    public DbSet<LoyaltyStats> LoyaltyStats { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId);
    }
}
