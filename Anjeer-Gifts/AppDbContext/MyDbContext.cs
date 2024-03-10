using Anjeer_Gifts.Configuration;
using Anjeer_Gifts.Models;
using Anjeer_Gifts.Models.Categories;
using Anjeer_Gifts.Models.GiftBoxes;
using Anjeer_Gifts.Models.Orders;
using Anjeer_Gifts.Models.Products;
using Anjeer_Gifts.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anjeer_Gifts.AppDbContext;

public class MyDbContext : DbContext
{
    public MyDbContext()
    {
        Database.EnsureCreated();
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<GiftBox> GiftBoxes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<MyLocation> MyLocation { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Constants.CONNECTION_STRING);
    }
}
