namespace Shipwreck.Aipri;

public sealed class Category : DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Category Clone()
        => new()
        {
            Id = Id,
            Name = Name,
        };
}
