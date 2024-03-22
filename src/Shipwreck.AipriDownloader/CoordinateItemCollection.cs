namespace Shipwreck.AipriDownloader;

public sealed class CoordinateItemCollection : DataItemCollection<CoordinateItem>
{
    public CoordinateItemCollection()
        : base(null) { }

    public CoordinateItemCollection(IEnumerable<CoordinateItem> items)
        : base(null)
    {
        Set(items);
    }

    internal CoordinateItemCollection(AipriVerseData? dataSet)
        : base(dataSet)
    {
    }
}
