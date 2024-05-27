namespace Shipwreck.Aipri;

public sealed class PartCollection : DataItemCollection<Part>
{
    public PartCollection()
        : base(null) { }

    public PartCollection(IEnumerable<Part> items)
        : base(null)
    {
        Set(items);
    }

    internal PartCollection(AipriDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Part? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);

    public Part? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);
}
