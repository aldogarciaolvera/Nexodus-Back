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
    public DbSet<DietTarget> DietTargets { get; set; } = null!;
    public DbSet<Food> Foods { get; set; } = null!;
    public DbSet<Meal> Meals { get; set; } = null!;
    public DbSet<MealItem> MealItems { get; set; } = null!;
    public DbSet<WaterLog> WaterLogs { get; set; } = null!;
    public DbSet<FastingLog> FastingLogs { get; set; } = null!;
    public DbSet<Finance> Finances { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Note> Notes { get; set; } = null!;
    public DbSet<ChecklistItem> ChecklistItems { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<Routine> Routines { get; set; } = null!;
    public DbSet<RoutineExercise> RoutineExercises { get; set; } = null!;
    public DbSet<WorkoutLog> WorkoutLogs { get; set; } = null!;
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

        modelBuilder.Entity<Note>()
            .HasMany(n => n.Checklist)
            .WithOne(c => c.Note)
            .HasForeignKey(c => c.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Exercise>()
            .HasIndex(e => e.ExternalId).IsUnique();

        modelBuilder.Entity<RoutineExercise>()
            .HasKey(re => new { re.RoutineId, re.ExerciseId });

        modelBuilder.Entity<RoutineExercise>()
            .HasOne(re => re.Routine)
            .WithMany(r => r.RoutineExercises)
            .HasForeignKey(re => re.RoutineId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoutineExercise>()
            .HasOne(re => re.Exercise)
            .WithMany()
            .HasForeignKey(re => re.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkoutLog>()
            .HasOne(wl => wl.User)
            .WithMany()
            .HasForeignKey(wl => wl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkoutLog>()
            .HasOne(wl => wl.Routine)
            .WithMany()
            .HasForeignKey(wl => wl.RoutineId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DietTarget>(entity =>
        {
            entity.HasIndex(dt => dt.UserId).IsUnique();
        });

        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasIndex(f => f.Barcode);
            entity.HasIndex(f => f.Name);
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasIndex(m => new { m.UserId, m.Date });
            entity.HasMany(m => m.Items)
                .WithOne(i => i.Meal)
                .HasForeignKey(i => i.MealId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WaterLog>(entity =>
        {
            entity.HasIndex(w => new { w.UserId, w.Date }).IsUnique();
        });

        modelBuilder.Entity<FastingLog>(entity =>
        {
            entity.HasIndex(fl => new { fl.UserId, fl.IsActive });
        });
    }
}
