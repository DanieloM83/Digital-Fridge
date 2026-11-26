using System.Globalization;
using Digital_Fridge.Models.Enums;
using Microsoft.Extensions.Localization;

namespace Digital_Fridge.Tests.Unit;

[Trait("Category", "Unit")]
public class LocalizationHelperTests
{
    private readonly IStringLocalizer<SharedResource> _localizer = new StubStringLocalizer();

    [Theory]
    [InlineData(UserRole.FamilyHead, "Role_FamilyHead")]
    [InlineData(UserRole.FamilyMember, "Role_FamilyMember")]
    [InlineData(UserRole.Admin, "Role_Admin")]
    [InlineData(UserRole.Guest, "Role_Guest")]
    public void GetRoleLabel_MapsAllUserRolesToExpectedKeys(UserRole role, string expectedKey)
    {
        var label = LocalizationHelper.GetRoleLabel(_localizer, role);
        Assert.Equal(expectedKey, label);
    }

    [Theory]
    [InlineData(UnitEnum.Grams, false, "Unit_Grams")]
    [InlineData(UnitEnum.Grams, true, "UnitShort_Grams")]
    [InlineData(UnitEnum.Tablespoons, false, "Unit_Tablespoons")]
    [InlineData(UnitEnum.Tablespoons, true, "UnitShort_Tablespoons")]
    [InlineData(UnitEnum.Pieces, true, "UnitShort_Pieces")]
    public void GetUnitLabel_SelectsLongOrShortKey(UnitEnum unit, bool shortForm, string expectedKey)
    {
        var label = LocalizationHelper.GetUnitLabel(_localizer, unit, shortForm);
        Assert.Equal(expectedKey, label);
    }

    [Theory]
    [InlineData(ProcessingLevel.Raw, "Processing_Raw")]
    [InlineData(ProcessingLevel.Processed, "Processing_Processed")]
    [InlineData(ProcessingLevel.UltraProcessed, "Processing_UltraProcessed")]
    public void GetProcessingLevelLabel_MapsAllLevels(ProcessingLevel level, string expectedKey)
    {
        var label = LocalizationHelper.GetProcessingLevelLabel(_localizer, level);
        Assert.Equal(expectedKey, label);
    }

    [Theory]
    [InlineData("Produce", "Category_Produce")]
    [InlineData("Dairy & Eggs", "Category_DairyAndEggs")]
    [InlineData("Meat & Poultry", "Category_MeatAndPoultry")]
    [InlineData("Pantry & Grains", "Category_PantryAndGrains")]
    [InlineData("Bakery", "Category_Bakery")]
    [InlineData("Condiments & Spices", "Category_CondimentsAndSpices")]
    [InlineData("Beverages", "Category_Beverages")]
    [InlineData("Custom Frozen Goods", "Custom Frozen Goods")]
    public void GetCategoryLabel_MapsSeededCategoriesAndFallsBackForCustom(string input, string expected)
    {
        var label = LocalizationHelper.GetCategoryLabel(_localizer, input);
        Assert.Equal(expected, label);
    }

    [Theory]
    [InlineData(1, "One:1")]
    [InlineData(2, "Few:2")]
    [InlineData(4, "Few:4")]
    [InlineData(5, "Many:5")]
    [InlineData(11, "Many:11")]
    [InlineData(14, "Many:14")]
    [InlineData(21, "One:21")]
    [InlineData(23, "Few:23")]
    public void FormatCount_Ukrainian_FollowsSlavicPluralRules(int count, string expected)
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("uk");
            var result = LocalizationHelper.FormatCount(_localizer, count, "One", "Few", "Many");
            Assert.Equal(expected, result);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [Theory]
    [InlineData(0, "Many:0")]
    [InlineData(1, "One:1")]
    [InlineData(2, "Many:2")]
    [InlineData(21, "Many:21")]
    public void FormatCount_English_UsesSingularOnlyForOne(int count, string expected)
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var result = LocalizationHelper.FormatCount(_localizer, count, "One", "Few", "Many");
            Assert.Equal(expected, result);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    private sealed class StubStringLocalizer : IStringLocalizer<SharedResource>
    {
        public LocalizedString this[string name] => new(name, name, resourceNotFound: false);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, $"{name}:{string.Join(",", arguments)}", resourceNotFound: false);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
