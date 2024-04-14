using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public sealed class Card : DataItem
{
    public int Id { get; set; }

    public string? ChapterId { get; set; }

    public string SealId { get; set; } = string.Empty;
    public string Coordinate { get; set; } = string.Empty;

    public string Character { get; set; } = string.Empty;

    [DefaultValue(null)]
    public string? Variant { get; set; }

    [DefaultValue("")]
    public string Song { get; set; } = string.Empty;

    [DefaultValue((short)0)]
    public short Point { get; set; }

    [DefaultValue(false)]
    public bool IsChance { get; set; }

    public string? Image1Url { get; set; }
    public string? Image2Url { get; set; }

    [JsonIgnore]
    internal bool IsImage1Loaded { get; set; }

    [JsonIgnore]
    internal bool IsImage2Loaded { get; set; }

    public Card Clone()
        => new()
        {
            Id = Id,
            ChapterId = ChapterId,
            SealId = SealId,
            Character = Character,
            Coordinate = Coordinate,
            Variant = Variant,
            Song = Song,
            Point = Point,
            IsChance = IsChance,
            Image1Url = Image1Url,
            Image2Url = Image2Url,
        };
}