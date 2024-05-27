using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public sealed class Part : DataItem
{
    public int Id { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImage1Loaded { get; set; }

    public Part Clone()
        => new()
        {
            Id = Id,
            Category = Category,
            Name = Name,
            Description = Description,
            ImageUrl = ImageUrl,
        };
}