using Digital_Fridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RecipeIngredient>(entity => {
            entity.HasKey(ri => new { ri.RecipeId, ri.ProductId });

            entity.HasOne(ri => ri.Recipe)
                  .WithMany(r => r.Ingredients)
                  .HasForeignKey(ri => ri.RecipeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(ri => ri.Quantity)
                  .HasPrecision(10, 2);
        });

        modelBuilder.Entity<Recipe>(entity => {
            entity.HasIndex(r => r.IsPublic);
            entity.HasIndex(r => r.Title);
        });

        modelBuilder.Entity<UserSession>(entity => {
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.ExpiresAtUtc);
        });
    }
}

