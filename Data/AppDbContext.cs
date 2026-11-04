using Digital_Fridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Family> Families => Set<Family>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<FridgeItem> FridgeItems => Set<FridgeItem>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<SavedRecipe> SavedRecipes => Set<SavedRecipe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ------------------------------------------
        // Household & Identity Domain
        // ------------------------------------------
        modelBuilder.Entity<Family>(entity =>
        {
            entity.HasMany(f => f.Members)
                  .WithOne(u => u.Family)
                  .HasForeignKey(u => u.FamilyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(f => f.FridgeItems)
                  .WithOne(fi => fi.Family)
                  .HasForeignKey(fi => fi.FamilyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(f => f.SavedRecipes)
                  .WithOne(sr => sr.Family)
                  .HasForeignKey(sr => sr.FamilyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.GoogleId).IsUnique();
            entity.HasIndex(u => u.Email);

            entity.Property(u => u.Role)
                  .HasConversion<string>();

            entity.HasMany(u => u.Sessions)
                  .WithOne(s => s.User)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.CreatedRecipes)
                  .WithOne(r => r.CreatedByUser)
                  .HasForeignKey(r => r.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(u => u.SavedRecipes)
                  .WithOne(sr => sr.User)
                  .HasForeignKey(sr => sr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.ExpiresAtUtc);
            entity.HasIndex(s => s.IsRevoked);
        });

        // ------------------------------------------
        // Product Taxonomy & Inventory Domain
        // ------------------------------------------
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasIndex(pc => pc.Name).IsUnique();

            entity.HasMany(pc => pc.Products)
                  .WithOne(p => p.Category)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Barcode);
            entity.HasIndex(p => p.Name);

            entity.Property(p => p.ProcessingLevel)
                  .HasConversion<string>();

            entity.Property(p => p.DefaultUnit)
                  .HasConversion<string>();

            entity.HasMany(p => p.FridgeItems)
                  .WithOne(fi => fi.Product)
                  .HasForeignKey(fi => fi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.RecipeIngredients)
                  .WithOne(ri => ri.Product)
                  .HasForeignKey(ri => ri.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FridgeItem>(entity =>
        {
            entity.Property(fi => fi.Quantity)
                  .HasPrecision(8, 2);

            entity.Property(fi => fi.Unit)
                  .HasConversion<string>();

            entity.Property(fi => fi.EntryMethod)
                  .HasConversion<string>();
        });

        // ------------------------------------------
        // Recipe & Curation Domain
        // ------------------------------------------
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasIndex(r => r.IsPublic);
            entity.HasIndex(r => r.Title);

            entity.HasMany(r => r.Ingredients)
                  .WithOne(ri => ri.Recipe)
                  .HasForeignKey(ri => ri.RecipeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.SavedBy)
                  .WithOne(sr => sr.Recipe)
                  .HasForeignKey(sr => sr.RecipeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(ri => new { ri.RecipeId, ri.ProductId });

            entity.Property(ri => ri.Quantity)
                  .HasPrecision(8, 2);

            entity.Property(ri => ri.Unit)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<SavedRecipe>(entity =>
        {
            entity.HasIndex(sr => new { sr.RecipeId, sr.UserId });
            entity.HasIndex(sr => new { sr.RecipeId, sr.FamilyId });
        });
    }
}
