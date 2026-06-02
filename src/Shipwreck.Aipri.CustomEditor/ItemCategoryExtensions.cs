using Shipwreck.ReflectionUtils;

namespace Shipwreck.Aipri.CustomEditor;

internal static class ItemCategoryExtensions
{
    public static bool TryParse(string s, out ItemCategory result)
        => EnumMemberDisplayNames<ItemCategory>.Default.TryParseValue(s, out result);

    public static string GetDisplayName(this ItemCategory v)
        => EnumMemberDisplayNames<ItemCategory>.Default.GetValue(v);
}
