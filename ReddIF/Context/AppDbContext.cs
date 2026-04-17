using Microsoft.EntityFrameworkCore;
using ReddIF.Models;

namespace ReddIF.Context;

    


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Community> Communities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Exemplo de configuração (opcional)
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Post>().ToTable("Posts");
        modelBuilder.Entity<Community>().ToTable("Communities");
    }
}
