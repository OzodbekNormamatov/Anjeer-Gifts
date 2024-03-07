using Anjeer_Gifts.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace Anjeer_Gifts.Database;

public class MyDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Set the connection string for your PostgreSQL database
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Anjeer-Gifts;Username=postgres;Password=0511");
    }
}
