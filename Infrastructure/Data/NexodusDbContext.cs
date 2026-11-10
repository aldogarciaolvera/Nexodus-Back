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
    public DbSet<WorkoutSession> WorkoutSessions { get; set; } = null!;
    public DbSet<WorkoutSet> WorkoutSets { get; set; } = null!;
    public DbSet<UserGamification> UserGamifications { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;
    public DbSet<RecipeStep> RecipeSteps { get; set; } = null!;
    public DbSet<PantryItem> PantryItems { get; set; } = null!;
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
            entity.Property(c => c.Type)
                .HasMaxLength(20)
                .HasDefaultValue("Money");

            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.UserId, c.Name });
            entity.HasIndex(c => c.Type);
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
            entity.HasIndex(f => f.CategoryId);

            entity.HasOne(f => f.FoodCategory)
                .WithMany(c => c.Foods)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
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

        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.HasIndex(ws => new { ws.UserId, ws.IsActive });
            entity.HasIndex(ws => ws.StartTime);

            entity.HasOne(ws => ws.User)
                .WithMany()
                .HasForeignKey(ws => ws.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ws => ws.Routine)
                .WithMany()
                .HasForeignKey(ws => ws.RoutineId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(ws => ws.Sets)
                .WithOne(s => s.WorkoutSession)
                .HasForeignKey(s => s.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkoutSet>(entity =>
        {
            entity.HasIndex(s => s.WorkoutSessionId);
            entity.HasIndex(s => s.ExerciseId);
            entity.HasIndex(s => new { s.ExerciseId, s.IsCompleted });

            entity.HasOne(s => s.Exercise)
                .WithMany()
                .HasForeignKey(s => s.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserGamification>(entity =>
        {
            entity.HasIndex(g => g.UserId).IsUnique();

            entity.HasOne(g => g.User)
                .WithOne()
                .HasForeignKey<UserGamification>(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.MealType);
            entity.HasIndex(r => r.DietType);
            entity.HasIndex(r => r.IsCommunity);
            entity.HasIndex(r => r.ParentRecipeId);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ParentRecipe)
                .WithMany(p => p.Variants)
                .HasForeignKey(r => r.ParentRecipeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(r => r.Ingredients)
                .WithOne(i => i.Recipe)
                .HasForeignKey(i => i.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Steps)
                .WithOne(s => s.Recipe)
                .HasForeignKey(s => s.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasIndex(ri => ri.RecipeId);
            entity.HasIndex(ri => ri.FoodId);

            entity.HasOne(ri => ri.Food)
                .WithMany()
                .HasForeignKey(ri => ri.FoodId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RecipeStep>(entity =>
        {
            entity.HasIndex(rs => new { rs.RecipeId, rs.StepNumber });
        });

        modelBuilder.Entity<PantryItem>(entity =>
        {
            entity.HasIndex(pi => pi.UserId);
            entity.HasIndex(pi => pi.FoodId);

            entity.HasOne(pi => pi.User)
                .WithMany()
                .HasForeignKey(pi => pi.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pi => pi.Food)
                .WithMany()
                .HasForeignKey(pi => pi.FoodId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
