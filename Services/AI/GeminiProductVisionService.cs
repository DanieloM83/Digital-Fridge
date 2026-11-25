using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Digital_Fridge.Models;
using Digital_Fridge.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Digital_Fridge.Services.AI;

public class GeminiProductVisionService : IProductVisionService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _modelName;
    private readonly ILogger<GeminiProductVisionService> _logger;

    public GeminiProductVisionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiProductVisionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"]
               ?? configuration["GEMINI_API_KEY"]
               ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        _modelName = configuration["Gemini:VisionModel"]
                  ?? "gemini-2.5-flash";
    }

    public async Task<ProductAnalysisResult?> AnalyzeProductImageAsync(
        string imageBase64OrDataUrl,
        IReadOnlyList<ProductCategory> availableCategories)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Falling back to default product mock.");
            return CreateFallbackResult(availableCategories);
        }

        try
        {
            // Extract pure base64 and mime type if data url
            var mimeType = "image/jpeg";
            var base64Data = imageBase64OrDataUrl;
            if (imageBase64OrDataUrl.Contains(","))
            {
                var parts = imageBase64OrDataUrl.Split(',', 2);
                base64Data = parts[1];
                if (parts[0].Contains("image/png")) mimeType = "image/png";
                else if (parts[0].Contains("image/webp")) mimeType = "image/webp";
            }

            var categoryNames = string.Join(", ", availableCategories.Select(c => c.Name));
            var systemPrompt = $"You are an expert food inventory analyzer. Analyze this photo of a grocery/food item. Identify its name, best matching category from this list: [{categoryNames}], estimated quantity, most appropriate measurement unit (one of: Grams, Milliliters, Pieces, Cloves, Sprigs, Pinch, Tablespoons, Teaspoons, Cups), processing level (Raw, MinimallyProcessed, ModeratelyProcessed, HighlyProcessed), estimated expiration date (YYYY-MM-DD), and barcode number if visible.";

            // Gemini v1beta generateContent endpoint
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

            var requestBody = new {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = systemPrompt },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = mimeType,
                                    data = base64Data
                                }
                            }
                        }
                    }
                },
                generationConfig = new {
                    response_mime_type = "application/json",
                    response_schema = new {
                        type = "OBJECT",
                        properties = new {
                            name = new { type = "STRING", description = "Standard grocery name" },
                            categoryName = new { type = "STRING", description = "Best matching category name from the provided list" },
                            quantity = new { type = "NUMBER", description = "Numeric count or weight" },
                            unit = new { type = "STRING", description = "Unit name matching UnitEnum" },
                            processingLevel = new { type = "STRING", description = "Raw, MinimallyProcessed, ModeratelyProcessed, or HighlyProcessed" },
                            estimatedExpirationDays = new { type = "INTEGER", description = "Estimated shelf life in days from today" },
                            barcode = new { type = "STRING", description = "Barcode if visible" }
                        },
                        required = new[] { "name", "categoryName", "quantity", "unit" }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini Vision API error ({StatusCode}): {Body}", response.StatusCode, errorBody);
                return CreateFallbackResult(availableCategories);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return CreateFallbackResult(availableCategories);

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(text)) return CreateFallbackResult(availableCategories);

            var parsed = JsonSerializer.Deserialize<GeminiProductDto>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed == null) return CreateFallbackResult(availableCategories);

            Enum.TryParse<UnitEnum>(parsed.Unit, true, out var unit);
            ProcessingLevel? processingLevel = null;
            if (Enum.TryParse<ProcessingLevel>(parsed.ProcessingLevel, true, out var pl))
            {
                processingLevel = pl;
            }

            DateOnly? expiration = parsed.EstimatedExpirationDays.HasValue
                ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(parsed.EstimatedExpirationDays.Value))
                : null;

            return new ProductAnalysisResult(
                parsed.Name ?? "Scanned Product",
                parsed.CategoryName,
                parsed.Quantity > 0 ? parsed.Quantity : 1,
                unit,
                processingLevel,
                expiration,
                parsed.Barcode
            );
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Gemini Vision API request timed out (25s). If you are in a network or region where Google AI endpoints are restricted, please verify your internet connection, VPN, or proxy settings.");
            return CreateFallbackResult(availableCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze product image with Gemini. Falling back to default.");
            return CreateFallbackResult(availableCategories);
        }
    }

    private static ProductAnalysisResult CreateFallbackResult(IReadOnlyList<ProductCategory> categories)
    {
        var categoryName = categories.FirstOrDefault()?.Name ?? "Produce";
        return new ProductAnalysisResult(
            "Fresh Produce Item",
            categoryName,
            1,
            UnitEnum.Pieces,
            ProcessingLevel.Raw,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            null
        );
    }

    private class GeminiProductDto
    {
        public string? Name { get; set; }
        public string? CategoryName { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? ProcessingLevel { get; set; }
        public int? EstimatedExpirationDays { get; set; }
        public string? Barcode { get; set; }
    }
}
