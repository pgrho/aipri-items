using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public sealed class Card : DataItem
{
    public int Id { get; set; }

    [DefaultValue(null)]
    public string? ChapterId { get; set; }

    [DefaultValue(null)]
    public int? BrandId { get; set; }

    public string SealId { get; set; } = string.Empty;

    [DefaultValue("")]
    public string Coordinate { get; set; } = string.Empty;

    [DefaultValue((double)int.MaxValue)]
    public double Order { get; set; } = int.MaxValue;

    #region Character

    private string? _Character;
    private int _CharacterId;

    [DefaultValue(null)]
    public string? Character
    {
        get
        {
            if (_Character == null && DataSet != null)
            {
                _Character = GetCharacter()?.Name;
            }
            return _Character;
        }
        set
        {
            if (value != _Character)
            {
                _Character = value;
                _CharacterId = 0;
            }
        }
    }

    [DefaultValue(0)]
    public int CharacterId
    {
        get
        {
            if (_CharacterId == 0 && DataSet != null)
            {
                _CharacterId = DataSet.Characters.GetByName(_Character)?.Id ?? 0;
            }
            return _CharacterId;
        }
        set
        {
            if (value != _CharacterId)
            {
                _CharacterId = value;
                _Character = null;
            }
        }
    }

    public Character? GetCharacter() => DataSet?.Characters.GetById(CharacterId);

    #endregion Character

    [DefaultValue(null)]
    public string? Variant { get; set; }

    #region Song

    private string? _Song;
    private int _SongId;

    [DefaultValue(null)]
    public string? Song
    {
        get
        {
            if (_Song == null && DataSet != null)
            {
                _Song = GetSong()?.Name;
            }
            return _Song;
        }
        set
        {
            if (value != _Song)
            {
                _Song = value;
                _SongId = 0;
            }
        }
    }

    [DefaultValue(0)]
    public int SongId
    {
        get
        {
            if (_SongId == 0 && DataSet != null)
            {
                _SongId = DataSet.Songs.GetByName(_Song)?.Id ?? 0;
            }
            return _SongId;
        }
        set
        {
            if (value != _SongId)
            {
                _SongId = value;
                _Song = null;
            }
        }
    }

    public Song? GetSong() => DataSet?.Songs.GetById(SongId);

    #endregion Song

    [DefaultValue((byte)0)]
    public byte Star { get; set; }

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
            BrandId = BrandId,
            SealId = SealId,
            Order = Order,
            Coordinate = Coordinate,
            _Character = _Character,
            _CharacterId = _CharacterId,
            Variant = Variant,
            _Song = _Song,
            _SongId = _SongId,
            Star = Star,
            Point = Point,
            IsChance = IsChance,
            Image1Url = Image1Url,
            Image2Url = Image2Url,
        };
}