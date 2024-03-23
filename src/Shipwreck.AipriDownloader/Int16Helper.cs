namespace Shipwreck.AipriDownloader;

internal static class Int16Helper
{
    public static short? PositiveOrNull(this short s)
        => s > 0 ? s : null;
}
