namespace Shipwreck.AipriDownloader;

public sealed class CoordinateCollection : DataItemCollection<Coordinate>
{
    public CoordinateCollection()
        : base(null) { }

    public CoordinateCollection(IEnumerable<Coordinate> items)
        : base(null)
    {
        Set(items);
    }

    internal CoordinateCollection(AipriVerseData? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Coordinate? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}
