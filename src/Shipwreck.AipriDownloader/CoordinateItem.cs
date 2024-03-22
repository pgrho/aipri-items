using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Shipwreck.AipriDownloader;

public sealed class CoordinateItem : DataItem
{
    public int Id { get; set; }

    public int CoordinateId { get; set; }

    public string SealId { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public short Point { get; set; }
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }

    public CoordinateItem Clone()
        => new()
        {
            Id = Id,
            CoordinateId = CoordinateId,
            SealId = SealId,
            Term = Term,
            Point = Point,
            ImageUrl = ImageUrl
        };
}