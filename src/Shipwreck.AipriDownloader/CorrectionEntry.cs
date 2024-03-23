namespace Shipwreck.AipriDownloader;

public sealed class CorrectionEntry<T> : DataItem
{
    public string Key { get; set; } = string.Empty;
    public T? Data { get; set; }
}
