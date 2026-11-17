using Digital_Fridge.Models;

namespace Digital_Fridge.Repositories;

public interface IStorageRepository
{
    Task<IReadOnlyList<FridgeItem>> GetFridgeItemsByFamilyIdAsync(Guid familyId);
    Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync();
    Task<IReadOnlyList<Product>> GetAllProductsAsync();
    Task<FridgeItem?> GetFridgeItemByIdAsync(Guid fridgeItemId);
    Task<FridgeItem> SaveFridgeItemAsync(FridgeItem item, Product product);
    Task<bool> DeleteFridgeItemAsync(Guid fridgeItemId);
}
