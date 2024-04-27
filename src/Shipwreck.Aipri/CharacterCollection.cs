namespace Shipwreck.Aipri;

public sealed class CharacterCollection : DataItemCollection<Character>
{
    public CharacterCollection()
        : base(null) { }

    public CharacterCollection(IEnumerable<Character> items)
        : base(null)
    {
        Set(items);
    }

    internal CharacterCollection(AipriDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Character? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);

    public Character? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}
