namespace Shipwreck.Aipri;

public sealed class CategoryCollection : DataItemCollection<Category>
{
    public CategoryCollection()
        : base(null) { }

    public CategoryCollection(IEnumerable<Category> items)
        : base(null)
    {
        Set(items);
    }

    internal CategoryCollection(AipriDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Category? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);

    public Category? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}
