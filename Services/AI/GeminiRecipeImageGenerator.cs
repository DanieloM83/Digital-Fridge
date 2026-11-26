using System.Text;
using System.Text.Json;

namespace Digital_Fridge.Services.AI;

public class GeminiRecipeImageGenerator : IRecipeImageGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _modelName;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<GeminiRecipeImageGenerator> _logger;

    public GeminiRecipeImageGenerator(
        HttpClient httpClient,
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<GeminiRecipeImageGenerator> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(120);
        _env = env;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"]
               ?? configuration["GEMINI_API_KEY"]
               ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        _modelName = configuration["Gemini:ImageModel"]
                  ?? "gemini-2.5-flash-image";
    }

    public async Task<string?> GenerateRecipeImageAsync(
        Guid recipeId,
        string title,
        string shortDescription,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Skipping recipe image generation.");
            return null;
        }

        try
        {
            var prompt = $"Appetizing, natural-light food photography of \"{title}\". {shortDescription}. Plated cleanly on a ceramic dish, shallow depth of field, no text or watermarks.";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent";

            var requestBody = new {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new {
                    responseModalities = new[] { "IMAGE" }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            var response = await _httpClient.SendAsync(request, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogWarning("Gemini Image API error ({StatusCode}) for '{Title}': {Body}", response.StatusCode, title, errorBody);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(responseJson);

            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return null;

            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("inlineData", out var inlineData) ||
                    part.TryGetProperty("inline_data", out inlineData))
                {
                    var base64 = inlineData.GetProperty("data").GetString();
                    if (string.IsNullOrWhiteSpace(base64)) continue;

                    var mimeType = inlineData.TryGetProperty("mimeType", out var mt)
                        ? mt.GetString()
                        : inlineData.TryGetProperty("mime_type", out var mt2) ? mt2.GetString() : "image/png";

                    var ext = mimeType switch {
                        "image/jpeg" => "jpg",
                        "image/webp" => "webp",
                        _ => "png"
                    };

                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var dir = Path.Combine(webRoot, "images", "generated");
                    Directory.CreateDirectory(dir);

                    var fileName = $"{recipeId}.{ext}";
                    var filePath = Path.Combine(dir, fileName);
                    await File.WriteAllBytesAsync(filePath, Convert.FromBase64String(base64), cts.Token);

                    return $"/images/generated/{fileName}";
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate image for recipe '{Title}'. Using fallback image.", title);
            return null;
        }
    }
}
