using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public sealed class Coordinate : DataItem
{
    public int Id { get; set; }

    public string? ChapterId { get; set; }
    public int? BrandId { get; set; }

    public byte? Star { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Group { get; set; }
    public string? Kind { get; set; }
    public DateOnly? Start { get; set; }
    public DateOnly? End { get; set; }

    [DefaultValue((double)int.MaxValue)]
    public double Order { get; set; } = int.MaxValue;

    [DefaultValue(false)]
    public bool HasChance { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }

    [JsonIgnore]
    internal bool IsThumbnailLoaded { get; set; }

    #region LinkedItemIds

    private List<int>? _LinkedItemIds;

    public IList<int> LinkedItemIds
    {
        get => _LinkedItemIds ??= new();
        set
        {
            if (value != _LinkedItemIds)
            {
                _LinkedItemIds?.Clear();
                ((List<int>)LinkedItemIds).AddRange(value ?? []);
            }
        }
    }

    public IEnumerable<CoordinateItem> GetLinkedItems()
    {
        if (_LinkedItemIds != null && DataSet is var ds && ds != null)
        {
            foreach (var id in _LinkedItemIds)
            {
                var c = ds.CoordinateItems.GetById(id);
                if (c != null)
                {
                    yield return c;
                }
            }
        }
    }

    #endregion LinkedItemIds

    public Coordinate Clone()
        => new()
        {
            Id = Id,
            ChapterId = ChapterId,
            BrandId = BrandId,
            Star = Star,
            Name = Name,
            HasChance = HasChance,
            Order = Order,
            ImageUrl = ImageUrl,
            ThumbnailUrl = ThumbnailUrl,
            Group = Group,
            Kind = Kind,
            Start = Start,
            End = End,
            _LinkedItemIds = _LinkedItemIds?.Count > 0 ? _LinkedItemIds?.ToList() : null,
        };
}