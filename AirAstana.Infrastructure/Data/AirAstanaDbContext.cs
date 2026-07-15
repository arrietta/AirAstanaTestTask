using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AirAstanaDbContext : DbContext
{
    public AirAstanaDbContext(DbContextOptions<AirAstanaDbContext> options) : base(options)
    {
    }

    public DbSet<Flight> Flights { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.Property(f => f.Origin).HasMaxLength(256).IsRequired();
            entity.Property(f => f.Destination).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Username).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Password).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasOne(u => u.Role).WithMany().HasForeignKey(u => u.RoleId);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(r => r.Code).HasMaxLength(256).IsRequired();
            entity.HasIndex(r => r.Code).IsUnique();
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Code = "User" },
            new Role { Id = 2, Code = "Moderator" }
        );
    }
}