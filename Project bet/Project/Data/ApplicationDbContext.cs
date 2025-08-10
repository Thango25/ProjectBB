using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Profile> Profiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Animal", IconClass = "fas fa-paw" },
            new Category { Id = 2, Name = "Electronics", IconClass = "fas fa-mobile-alt" },
            new Category { Id = 3, Name = "Jewellery", IconClass = "fas fa-gem" },
            new Category { Id = 4, Name = "Wallets", IconClass = "fas fa-wallet" },
            new Category { Id = 5, Name = "Keys", IconClass = "fas fa-key" },
            new Category { Id = 6, Name = "Bags", IconClass = "fas fa-suitcase" },
            new Category { Id = 7, Name = "Eyewear", IconClass = "fas fa-glasses" },
            new Category { Id = 8, Name = "Clothing", IconClass = "fas fa-tshirt" },
            new Category { Id = 9, Name = "Documents", IconClass = "fas fa-book" },
            new Category { Id = 10, Name = "Bicycles", IconClass = "fas fa-bicycle" },
            new Category { Id = 11, Name = "Musical Instruments", IconClass = "fas fa-music" },
            new Category { Id = 12, Name = "Toys & Games", IconClass = "fas fa-gamepad" }
        );

       
    }


}
