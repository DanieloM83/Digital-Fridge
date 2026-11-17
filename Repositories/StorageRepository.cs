using Digital_Fridge.Data;
using Digital_Fridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital_Fridge.Repositories;

public class StorageRepository : IStorageRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public StorageRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<FridgeItem>> GetFridgeItemsByFamilyIdAsync(Guid familyId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.FridgeItems
            .AsNoTracking()
            .Include(fi => fi.Product)
                .ThenInclude(p => p!.Category)
            .Where(fi => fi.FamilyId == familyId)
            .OrderBy(fi => fi.Product != null ? fi.Product.Name : string.Empty)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetAllProductsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<FridgeItem?> GetFridgeItemByIdAsync(Guid fridgeItemId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.FridgeItems
            .AsNoTracking()
            .Include(fi => fi.Product)
                .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(fi => fi.FridgeItemId == fridgeItemId);
    }

    public async Task<FridgeItem> SaveFridgeItemAsync(FridgeItem item, Product productDetails)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Ensure product exists or is updated
        Product? product = null;
        if (item.ProductId != Guid.Empty)
        {
            product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
        }

        if (product == null && !string.IsNullOrWhiteSpace(productDetails.Name))
        {
            // Look up by name (case-insensitive) or create new product
            product = await context.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == productDetails.Name.Trim().ToLower());
        }

        if (product != null)
        {
            product.Name = productDetails.Name.Trim();
            product.CategoryId = productDetails.CategoryId;
            product.ProcessingLevel = productDetails.ProcessingLevel;
            product.DefaultUnit = productDetails.DefaultUnit;
            product.Barcode = string.IsNullOrWhiteSpace(productDetails.Barcode) ? null : productDetails.Barcode.Trim();
            context.Products.Update(product);
        }
        else
        {
            product = new Product {
                ProductId = Guid.NewGuid(),
                Name = productDetails.Name.Trim(),
                CategoryId = productDetails.CategoryId,
                ProcessingLevel = productDetails.ProcessingLevel,
                DefaultUnit = productDetails.DefaultUnit,
                Barcode = string.IsNullOrWhiteSpace(productDetails.Barcode) ? null : productDetails.Barcode.Trim()
            };
            await context.Products.AddAsync(product);
        }

        await context.SaveChangesAsync();

        // 2. Add or update FridgeItem
        item.ProductId = product.ProductId;
        var existingItem = await context.FridgeItems.FirstOrDefaultAsync(fi => fi.FridgeItemId == item.FridgeItemId);
        if (existingItem != null)
        {
            existingItem.Quantity = item.Quantity;
            existingItem.Unit = item.Unit;
            existingItem.ExpirationDate = item.ExpirationDate;
            existingItem.EntryMethod = item.EntryMethod;
            context.FridgeItems.Update(existingItem);
            await context.SaveChangesAsync();
            return existingItem;
        }
        else
        {
            if (item.FridgeItemId == Guid.Empty)
            {
                item.FridgeItemId = Guid.NewGuid();
            }
            item.CreatedAt = DateTime.UtcNow;
            await context.FridgeItems.AddAsync(item);
            await context.SaveChangesAsync();
            return item;
        }
    }

    public async Task<bool> DeleteFridgeItemAsync(Guid fridgeItemId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.FridgeItems.FindAsync(fridgeItemId);
        if (item == null) return false;

        context.FridgeItems.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }
}
