namespace Shipwreck.Aipri;

public sealed class Character : DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Character Clone()
        => new()
        {
            Id = Id,
            Name = Name,
        };
}
