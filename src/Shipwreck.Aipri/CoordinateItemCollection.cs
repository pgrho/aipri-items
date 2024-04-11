namespace Shipwreck.Aipri;

public sealed class CoordinateItemCollection : DataItemCollection<CoordinateItem>
{
    public CoordinateItemCollection()
        : base(null) { }

    public CoordinateItemCollection(IEnumerable<CoordinateItem> items)
        : base(null)
    {
        Set(items);
    }

    internal CoordinateItemCollection(AipriVerseDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public CoordinateItem? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);
}
