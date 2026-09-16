using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Infrastructure.Data;

public class NexodusDbContext : DbContext
{
    public NexodusDbContext(DbContextOptions<NexodusDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TodoList> TodoList { get; set; } = null!;
    public DbSet<Workout> Workouts { get; set; } = null!;
    public DbSet<Diet> Diet { get; set; } = null!;
    public DbSet<Finance> Finances { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Opcional: Configuraciones adicionales con Fluent API
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
        });

        modelBuilder.Entity<Finance>()
            .HasOne(f => f.Category)
            .WithMany(c => c.Finances)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
