using System.Text.Json.Serialization;

namespace Shipwreck.AipriDownloader;

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }
}
