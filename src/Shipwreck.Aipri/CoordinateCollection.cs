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
    public Coordinate? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);

    public Coordinate? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}

public sealed class CardCollection : DataItemCollection<Card>
{
    public CardCollection()
        : base(null) { }

    public CardCollection(IEnumerable<Card> items)
        : base(null)
    {
        Set(items);
    }

    internal CardCollection(AipriVerseDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Card? GetById(int id)
        => this.FirstOrDefault(e => e.Id == id);

    public Card? GetBySealId(string sealId)
        => this.FirstOrDefault(e => e.SealId == sealId);

    public Card? GetByName(string coordinate, string character)
        => this.FirstOrDefault(e => e.Coordinate == coordinate && e.Character == character);
}
