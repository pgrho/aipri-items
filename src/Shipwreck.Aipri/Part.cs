using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public sealed class Part : DataItem
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImage1Loaded { get; set; }

    public Part Clone()
        => new()
        {
            Id = Id,
            CategoryId = CategoryId,
            Name = Name,
            Description = Description,
            ImageUrl = ImageUrl,
        };
}