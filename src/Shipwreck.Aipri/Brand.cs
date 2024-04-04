using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public sealed class Brand : DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }

    public Brand Clone()
        => new()
        {
            Id = Id,
            Name = Name,
            ImageUrl = ImageUrl
        };
}