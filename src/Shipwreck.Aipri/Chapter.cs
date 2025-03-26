using System.Text.RegularExpressions;

namespace Shipwreck.Aipri;

public sealed partial class Chapter : DataItem
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

    [GeneratedRegex("^(?:ring)?[1-9]$")]
    private static partial Regex ChapterIdPattern();

    public static long ParseIdOrder(string id)
    {
        if (TryParseId(id, out var y, out var n))
        {
            return y * 10 + n;
        }
        return int.MaxValue;
    }

    public static bool TryParseId(string id, out int year, out int no)
    {
        if (id != null)
        {
            var m = ChapterIdPattern().Match(id);
            if (m.Success)
            {
                year = id[0] switch
                {
                    'r' or 'R' => 2,
                    >= '1' and <= '9' => 1,
                    _ => int.MaxValue
                };
                no = id[id.Length - 1] - '0';

                return true;
            }
        }

        year = no = int.MaxValue;
        return false;
    }
}
