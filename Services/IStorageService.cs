using Digital_Fridge.Models;

namespace Digital_Fridge.Services;

public interface IStorageService
{
    Task<IReadOnlyList<FridgeItem>> GetFamilyFridgeItemsAsync(Guid familyId);
    Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync();
    Task<IReadOnlyList<Product>> GetProductsAsync();
    Task<FridgeItem?> GetFridgeItemAsync(Guid fridgeItemId);
    Task<FridgeItem> SaveFridgeItemAsync(FridgeItem item, Product product);
    Task<bool> DeleteFridgeItemAsync(Guid fridgeItemId);
}
