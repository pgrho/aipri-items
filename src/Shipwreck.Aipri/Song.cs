namespace Shipwreck.Aipri;

public sealed class Song : DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Song Clone()
        => new()
        {
            Id = Id,
            Name = Name,
        };
}
