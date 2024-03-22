namespace Shipwreck.AipriDownloader;

public sealed class Chapter : DataItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Chapter Clone()
        => new()
        {
            Id = Id,
            Name = Name,
        };
}