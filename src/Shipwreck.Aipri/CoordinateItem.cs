using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Shipwreck.Aipri;

public sealed class CoordinateItem : DataItem
{
    public int Id { get; set; }

    public int CoordinateId { get; set; }

    public Coordinate? GetCoordinate() => DataSet?.Coordinates.GetById(CoordinateId);

    public string SealId { get; set; } = string.Empty;

    #region Category

    private string? _Term;
    private int _CategoryId;

    public string? Term
    {
        get
        {
            if (_Term == null && DataSet != null)
            {
                _Term = GetCategory()?.Name;
            }
            return _Term;
        }
        set
        {
            if (value != _Term)
            {
                _Term = value;
                _CategoryId = 0;
            }
        }
    }

    public int CategoryId
    {
        get
        {
            if (_CategoryId == 0 && DataSet != null)
            {
                _CategoryId = DataSet.Categories.GetByName(_Term)?.Id ?? 0;
            }
            return _CategoryId;
        }
        set
        {
            if (value != _CategoryId)
            {
                _CategoryId = value;
                _Term = null;
            }
        }
    }

    public Category? GetCategory() => DataSet?.Categories.GetById(CategoryId);

    #endregion Category

    public short Point { get; set; }
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal bool IsImageLoaded { get; set; }

    [JsonIgnore]
    internal bool IsCurrentRun { get; set; }

    public CoordinateItem Clone()
        => new()
        {
            Id = Id,
            CoordinateId = CoordinateId,
            SealId = SealId,
            _Term = _Term,
            _CategoryId = _CategoryId,
            Point = Point,
            ImageUrl = ImageUrl
        };
}