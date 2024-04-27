namespace Shipwreck.Aipri;

public sealed class CardCollection : DataItemCollection<Card>
{
    public CardCollection()
        : base(null) { }

    public CardCollection(IEnumerable<Card> items)
        : base(null)
    {
        Set(items);
    }

    internal CardCollection(AipriDataSet? dataSet)
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
