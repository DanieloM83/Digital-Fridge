using System.Globalization;
using Digital_Fridge.Models.Enums;
using Microsoft.Extensions.Localization;

namespace Digital_Fridge;

/// <summary>
/// Centralized localization helpers for enums, seeded categories, and culture-aware pluralization.
/// </summary>
public static class LocalizationHelper
{
    public static string GetRoleLabel(IStringLocalizer<SharedResource> L, UserRole role) => role switch {
        UserRole.FamilyHead => L["Role_FamilyHead"],
        UserRole.FamilyMember => L["Role_FamilyMember"],
        UserRole.Admin => L["Role_Admin"],
        _ => L["Role_Guest"]
    };

    public static string GetUnitLabel(IStringLocalizer<SharedResource> L, UnitEnum unit, bool shortForm = false)
    {
        var prefix = shortForm ? "UnitShort_" : "Unit_";
        return unit switch {
            UnitEnum.Grams => L[$"{prefix}Grams"],
            UnitEnum.Milliliters => L[$"{prefix}Milliliters"],
            UnitEnum.Pieces => L[$"{prefix}Pieces"],
            UnitEnum.Cloves => L[$"{prefix}Cloves"],
            UnitEnum.Sprigs => L[$"{prefix}Sprigs"],
            UnitEnum.Pinch => L[$"{prefix}Pinch"],
            UnitEnum.Tablespoons => L[$"{prefix}Tablespoons"],
            UnitEnum.Teaspoons => L[$"{prefix}Teaspoons"],
            UnitEnum.Cups => L[$"{prefix}Cups"],
            _ => unit.ToString()
        };
    }

    public static string GetProcessingLevelLabel(IStringLocalizer<SharedResource> L, ProcessingLevel level) => level switch {
        ProcessingLevel.Raw => L["Processing_Raw"],
        ProcessingLevel.Processed => L["Processing_Processed"],
        ProcessingLevel.UltraProcessed => L["Processing_UltraProcessed"],
        _ => level.ToString()
    };

    public static string GetCategoryLabel(IStringLocalizer<SharedResource> L, string categoryName) => categoryName switch {
        "Produce" => L["Category_Produce"],
        "Dairy & Eggs" => L["Category_DairyAndEggs"],
        "Meat & Poultry" => L["Category_MeatAndPoultry"],
        "Pantry & Grains" => L["Category_PantryAndGrains"],
        "Bakery" => L["Category_Bakery"],
        "Condiments & Spices" => L["Category_CondimentsAndSpices"],
        "Beverages" => L["Category_Beverages"],
        _ => categoryName
    };

    public static string FormatCount(
        IStringLocalizer<SharedResource> L,
        int count,
        string oneKey,
        string fewKey,
        string manyKey)
    {
        var abs = Math.Abs(count);
        string key;

        if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "uk")
        {
            var mod10 = abs % 10;
            var mod100 = abs % 100;
            if (mod10 == 1 && mod100 != 11)
            {
                key = oneKey;
            }
            else if (mod10 is >= 2 and <= 4 && mod100 is < 12 or > 14)
            {
                key = fewKey;
            }
            else
            {
                key = manyKey;
            }
        }
        else
        {
            key = abs == 1 ? oneKey : manyKey;
        }

        return L[key, count];
    }
}
