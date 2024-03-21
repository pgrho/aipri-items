using System.Text.Json.Serialization;

namespace Shipwreck.AipriDownloader;

public class CoordinateItem
{
    public int Id { get; set; }
     
    public int  CoordinateId { get; set; }

    public string SealId { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public short  Point { get; set; } 
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }
}
