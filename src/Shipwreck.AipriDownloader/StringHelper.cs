namespace Shipwreck.AipriDownloader;

internal static class StringHelper
{
    public static string? TrimOrNull(this string? s)
        => string.IsNullOrEmpty(s) ? null : s.Trim();
}
