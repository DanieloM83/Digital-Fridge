using Digital_Fridge.Models;
using Digital_Fridge.Repositories;

namespace Digital_Fridge.Services;

public class StorageService : IStorageService
{
    private readonly IStorageRepository _storageRepository;

    public StorageService(IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
    }

    public async Task<IReadOnlyList<FridgeItem>> GetFamilyFridgeItemsAsync(Guid familyId)
    {
        return await _storageRepository.GetFridgeItemsByFamilyIdAsync(familyId);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync()
    {
        return await _storageRepository.GetAllCategoriesAsync();
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync()
    {
        return await _storageRepository.GetAllProductsAsync();
    }

    public async Task<FridgeItem?> GetFridgeItemAsync(Guid fridgeItemId)
    {
        return await _storageRepository.GetFridgeItemByIdAsync(fridgeItemId);
    }

    public async Task<FridgeItem> SaveFridgeItemAsync(FridgeItem item, Product product)
    {
        return await _storageRepository.SaveFridgeItemAsync(item, product);
    }

    public async Task<bool> DeleteFridgeItemAsync(Guid fridgeItemId)
    {
        return await _storageRepository.DeleteFridgeItemAsync(fridgeItemId);
    }
}
