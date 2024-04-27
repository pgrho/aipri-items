namespace Shipwreck.Aipri;

public sealed class SongCollection : DataItemCollection<Song>
{
    public SongCollection()
        : base(null) { }

    public SongCollection(IEnumerable<Song> items)
        : base(null)
    {
        Set(items);
    }

    internal SongCollection(AipriDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Song? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);

    public Song? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}