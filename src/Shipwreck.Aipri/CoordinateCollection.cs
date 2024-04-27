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

    internal CoordinateCollection(AipriDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Coordinate? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);

    public Coordinate? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}
