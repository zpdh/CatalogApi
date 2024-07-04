using CatalogApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Data;

public class DataContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Category> Categories { get; set; }

    public DbSet<Product> Products { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
    : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
