using System.Globalization;
using System.Text.RegularExpressions;
using Digital_Fridge.Models.Enums;
using Digital_Fridge.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Digital_Fridge.Tests.Integration;

[Trait("Category", "Integration")]
public class LocalizationResourceTests(TestAppFactory factory) : IClassFixture<TestAppFactory>
{
    [Theory]
    [InlineData("uk", "Nav_Recipes", "Рецепти")]
    [InlineData("en", "Nav_Recipes", "Recipes")]
    [InlineData("uk", "Nav_Workspace", "Робоча зона")]
    [InlineData("en", "Nav_Workspace", "Workspace")]
    [InlineData("uk", "WorkspaceNav_Storage", "Комора")]
    [InlineData("en", "WorkspaceNav_Storage", "Storage")]
    public void StringLocalizer_ResolvesKeyFromRealAppServices(string culture, string key, string expected)
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            using var scope = factory.Services.CreateScope();
            var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

            var value = localizer[key];

            Assert.False(value.ResourceNotFound, $"Resource '{key}' not found at '{value.SearchedLocation}'.");
            Assert.Equal(expected, value.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [Fact]
    public void AllReferencedResourceKeys_ExistInBothUkrainianAndEnglish()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var files = Directory.GetFiles(Path.Combine(repoRoot, "Components"), "*.razor", SearchOption.AllDirectories)
            .Concat([Path.Combine(repoRoot, "LocalizationHelper.cs")]);

        var regex = new Regex(@"L\[""([^""]+)""");
        var keys = new SortedSet<string>();
        foreach (var file in files)
        {
            foreach (Match m in regex.Matches(File.ReadAllText(file)))
            {
                keys.Add(m.Groups[1].Value);
            }
        }

        foreach (var unit in Enum.GetValues<UnitEnum>())
        {
            keys.Add($"Unit_{unit}");
            keys.Add($"UnitShort_{unit}");
        }
        foreach (var level in Enum.GetValues<ProcessingLevel>())
        {
            keys.Add($"Processing_{level}");
        }
        foreach (var role in Enum.GetValues<UserRole>())
        {
            keys.Add($"Role_{role}");
        }

        using var scope = factory.Services.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

        var original = CultureInfo.CurrentUICulture;
        try
        {
            foreach (var culture in new[] { "uk", "en" })
            {
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
                foreach (var key in keys)
                {
                    var localized = localizer[key];
                    Assert.False(localized.ResourceNotFound, $"Missing '{key}' for '{culture}'.");
                    Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Empty '{key}' for '{culture}'.");
                }
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }
}
