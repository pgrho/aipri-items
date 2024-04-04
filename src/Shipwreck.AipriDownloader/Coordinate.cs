using System.Text.Json.Serialization;

namespace Shipwreck.AipriDownloader;

public sealed class Coordinate : DataItem
{
    public int Id { get; set; }

    public string? ChapterId { get; set; }
    public int? BrandId { get; set; }

    public byte? Star { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Kind { get; set; }
    public DateOnly? Start { get; set; }
    public DateOnly? End { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }

    [JsonIgnore]
    internal bool IsThumbnailLoaded { get; set; }

    public Coordinate Clone()
        => new()
        {
            Id = Id,
            ChapterId = ChapterId,
            BrandId = BrandId,
            Star = Star,
            Name = Name,
            ImageUrl = ImageUrl,
            ThumbnailUrl = ThumbnailUrl,
            Kind = Kind,
            Start = Start,
            End = End,
        };
}