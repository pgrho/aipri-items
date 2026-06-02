namespace Shipwreck.Aipri.CustomEditor;

internal static class Int32Helper
{
    public static int? PositiveOrNull(this int v)
        => v > 0 ? v : null;
}