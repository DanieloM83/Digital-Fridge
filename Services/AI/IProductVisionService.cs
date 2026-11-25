using Digital_Fridge.Models;

namespace Digital_Fridge.Services.AI;

public interface IProductVisionService
{
    Task<ProductAnalysisResult?> AnalyzeProductImageAsync(string imageBase64OrDataUrl, IReadOnlyList<ProductCategory> availableCategories);
}
