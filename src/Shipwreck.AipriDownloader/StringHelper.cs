using System.Text.RegularExpressions;

namespace Shipwreck.AipriDownloader;

internal static class StringHelper
{
    public static string Trim2(this string s)
        => Regex.Replace(s, @"[\s\t\r\n\u3000\u200b]+", m => m.Index == 0 || m.Index + m.Length == s.Length ? "" : " ");

    public static string? TrimOrNull(this string? s)
        => string.IsNullOrEmpty(s) ? null : s.Trim2();
}