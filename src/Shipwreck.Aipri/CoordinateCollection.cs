namespace Shipwreck.Aipri;

public sealed class CoordinateCollection : DataItemCollection<Coordinate>
{
    public CoordinateCollection()
        : base(null) { }

    public CoordinateCollection(IEnumerable<Coordinate> items)
        : base(null)
    {
        Set(items);
    }

    internal CoordinateCollection(AipriVerseDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Coordinate? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}
