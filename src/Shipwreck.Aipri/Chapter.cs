namespace Shipwreck.Aipri;

public sealed class Chapter : DataItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly? Start { get; set; }
    public DateOnly? End { get; set; }

    public Chapter Clone()
        => new()
        {
            Id = Id,
            Name = Name,
            Start = Start,
            End = End,
        };
}