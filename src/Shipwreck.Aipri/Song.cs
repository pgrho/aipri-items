namespace Shipwreck.Aipri;

public sealed class Song : DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    #region SingerIds

    private List<int>? _SingerIds;

    public IList<int> SingerIds
    {
        get => _SingerIds ??= new();
        set
        {
            if (value != _SingerIds)
            {
                _SingerIds?.Clear();
                ((List<int>)SingerIds).AddRange(value ?? []);
            }
        }
    }

    public IEnumerable<Character> GetSingers()
    {
        if (_SingerIds != null && DataSet is var ds && ds != null)
        {
            foreach (var id in _SingerIds)
            {
                var c = ds.Characters.GetById(id);
                if (c != null)
                {
                    yield return c;
                }
            }
        }
    }

    #endregion SingerIds

    public Song Clone()
        => new()
        {
            Id = Id,
            Name = Name,
            _SingerIds = _SingerIds?.ToList(),
        };
}