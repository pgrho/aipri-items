using System.Text.Json.Serialization;

namespace Shipwreck.AipriDownloader;

public sealed class Coordinate : DataItem
{
    public int Id { get; set; }

    public string? ChapterId { get; set; }
    public int? BrandId { get; set; }

    public string Star { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }

    public Coordinate Clone()
        => new()
        {
            Id = Id,
            ChapterId = ChapterId,
            BrandId = BrandId,
            Star = Star,
            Name = Name,
            ImageUrl = ImageUrl
        };
}