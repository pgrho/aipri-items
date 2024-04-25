using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Shipwreck.AipriDownloader;

internal class Program
{
    private const string VERSE_HOME = "https://aipri.jp/verse/";
    private const string ITEM_FORMAT = VERSE_HOME + "item/{0}.html";

    private const string HIMITSU_HOME = "https://aipri.jp/";
    private const string CARD_FORMAT = HIMITSU_HOME + "card/{0}.html";

    static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var d = new DownloadContext();

        d.ClearSubdirectories();

        foreach (var c in d.DataSet.Coordinates)
        {
            c.LinkedItemIds.Clear();
        }

        await ParseItemAsync(d, string.Format(ITEM_FORMAT, "1"), true).ConfigureAwait(false);

        foreach (var ch in d.DataSet.VerseChapters.Where(e => e.Id != "1"))
        {
            await ParseItemAsync(d, string.Format(ITEM_FORMAT, ch.Id), false).ConfigureAwait(false);
        }

        await ParseCardAsync(d, string.Format(CARD_FORMAT, "1"), true).ConfigureAwait(false);

        foreach (var ch in d.DataSet.HimitsuChapters.Where(e => e.Id != "1"))
        {
            await ParseCardAsync(d, string.Format(CARD_FORMAT, ch.Id), false).ConfigureAwait(false);
        }

        // 既知の誤記載を訂正する
        await CorrectKnownTypoAsync(d).ConfigureAwait(false);

        await AddCardInfoAsync(d).ConfigureAwait(false);


        var comparer = StringComparer.Create(
            CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

        var chanceCoords = d.DataSet.Cards.Where(e => e.IsChance).Select(e => e.Coordinate).ToHashSet(comparer);
        foreach (var c in d.DataSet.Coordinates)
        {
            c.HasChance = chanceCoords.Contains(c.Name);
        }
    }

    static async Task ParseItemAsync(DownloadContext d, string itemUrl, bool parseChapters)
    {
        var url = new Uri(itemUrl);
        using var html = await d.GetAsync(itemUrl).ConfigureAwait(false);

        var doc = new HtmlDocument();
        doc.Load(html);

        var sn = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

        var chapterKey = Path.GetFileNameWithoutExtension(itemUrl);
        Chapter? chapter = d.DataSet.VerseChapters.FirstOrDefault(e => e.Id == chapterKey);

        if (!string.IsNullOrEmpty(sn)
            && Regex.Match(sn, "^(【(?<coord>[^】]+)】|(?<coord>.+))コーデアイテム") is var m
            && m.Success)
        {
            var ck = Path.GetFileNameWithoutExtension(itemUrl);
            var cn = m.Groups["coord"].Value;

            chapter = d.AddVerseChapter(ck, cn);
            Console.WriteLine(" - VerseChapter[{0}]: {1}", ck, cn);
        }

        var nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        if (parseChapters)
        {
            foreach (HtmlNodeNavigator cNode in nav.Select("//ul[@class='toggleList toggleList--pageSelect']/li/a/@href"))
            {
                var v = Path.GetFileNameWithoutExtension(cNode.GetAttribute("href", null));
                var t = cNode.Value?.Trim();
                d.AddVerseChapter(v, t ?? v);
                Console.WriteLine("   - VerseChapter[{0}]: {1}", v, t ?? v);
            }
        }

        nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        var oldCoordinates = d.DataSet.Coordinates.Where(e => e.ChapterId == chapter?.Id).ToList();
        var oldIds = oldCoordinates.Select(e => e.Id).ToList();
        var oldCoordinateItems = d.DataSet.CoordinateItems.Where(e => oldIds.Contains(e.CoordinateId)).ToList();

        var newCoordinates = new HashSet<Coordinate>();
        var newCoordinateItems = new HashSet<CoordinateItem>();

        foreach (HtmlNodeNavigator cNode in nav.Select("//div[@class='grid__item' or starts-with(@class, 'grid__item')]//a[@data-modal]"))
        {
            var modal = cNode.SelectSingleNode("@data-modal")?.Value;

            var bndImg = cNode.SelectSingleNode("@data-brand")?.Value;
            var bndName = cNode.SelectSingleNode("@data-brandname")?.Value;

            var b = string.IsNullOrEmpty(bndName) ? null
                : await d.AddBrandAsync(bndName, new Uri(url, bndImg).ToString()).ConfigureAwait(false);

            var title = cNode.SelectSingleNode("@data-name")?.Value;
            var star = cNode.SelectSingleNode("@data-icn")?.Value is string icn
                    && Regex.Match(Path.GetFileNameWithoutExtension(icn), "^icn_star\\d$") is var icnM
                    && icnM.Success
                    ? icnM.Value.Last() - '0'
                    : (int?)null;

            async Task addItemAsync(Coordinate coord, string? term, string? iUrl, string? iid, short point)
            {
                var eid = iUrl != null && Regex.Match(iUrl, "\\/Item_ID(\\d+)\\.webp$") is var em && em.Success ? int.Parse(em.Groups[1].Value)
                    : (int?)null;

                var first = (string.IsNullOrEmpty(iid)
                    ? null
                    : d.DataSet.CoordinateItems.FirstOrDefault(e => e.SealId == iid && e.IsCurrentRun))
                    ?? (d.DataSet.CoordinateItems.FirstOrDefault(e => e.GetCoordinate()?.Name == coord.Name && e.Term == term));

                if (first == null || first.CoordinateId == coord.Id)
                {
                    var item = await d.AddItemAsync(coord, iid, term, point, new Uri(url, iUrl).ToString(), eid).ConfigureAwait(false);

                    item.IsCurrentRun = true;

                    Console.WriteLine("     - Item[{0}]: {1}", item.Id, item.Term);

                    newCoordinateItems.Add(item);
                }
                else
                {
                    coord.LinkedItemIds.Add(first.Id);
                    Console.WriteLine("     - Item[{0}]: {1} (Linked to {2})", first.Id, first.Term, d.DataSet.Coordinates.GetById(first.CoordinateId)?.Name);
                }
            }

            if (modal == "item")
            {
                var imgUrl = cNode.SelectSingleNode("@data-img")?.Value;

                if (string.IsNullOrEmpty(title))
                {
                    continue;
                }

                var coord = await d.AddCoordinateAsync(chapter, b, title, star, new Uri(url, imgUrl).ToString()).ConfigureAwait(false);

                if (newCoordinates.Add(coord))
                {
                    Console.WriteLine("   - Coordinate[{0}]: {1}", coord.Id, coord.Name);
                }

                var thUrl = cNode.SelectSingleNode("./img/@src")?.Value;
                if (thUrl != null)
                {
                    thUrl = new Uri(url, thUrl).ToString();
                    if (coord.ThumbnailUrl != thUrl || !coord.IsThumbnailLoaded)
                    {
                        coord.ThumbnailUrl = await d.GetOrCopyImageAsync(thUrl, "coordinates", coord.Id, "-thumb").ConfigureAwait(false);
                        coord.IsThumbnailLoaded = true;
                    }
                }

                for (var n = 1; n <= 4; n++)
                {
                    var term = cNode.SelectSingleNode("@data-term" + n)?.Value?.Trim();
                    if (string.IsNullOrEmpty(term))
                    {
                        break;
                    }
                    var iUrl = cNode.SelectSingleNode("@data-img" + n)?.Value?.Trim();
                    var iid = cNode.SelectSingleNode("@data-id" + n)?.Value?.Trim();
                    var point = short.TryParse(cNode.SelectSingleNode("@data-point" + n)?.Value?.Trim(), out var pv) ? pv : (short)0;

                    await addItemAsync(coord, term, iUrl, iid, point).ConfigureAwait(false);
                }
            }
            else if (modal == "special")
            {
                if (string.IsNullOrEmpty(title))
                {
                    continue;
                }

                var coord = await d.AddCoordinateAsync(chapter, b, title, star, null).ConfigureAwait(false);

                if (newCoordinates.Add(coord))
                {
                    Console.WriteLine("   - Coordinate[{0}]: {1}", coord.Id, coord.Name);
                }

                var term = cNode.SelectSingleNode("@data-term")?.Value?.Trim();
                if (string.IsNullOrEmpty(term))
                {
                    break;
                }
                var iUrl = cNode.SelectSingleNode("@data-img")?.Value?.Trim();
                var iid = cNode.SelectSingleNode("@data-id")?.Value?.Trim();
                var point = short.TryParse(cNode.SelectSingleNode("@data-point")?.Value?.Trim(), out var pv) ? pv : (short)0;

                await addItemAsync(coord, term, iUrl, iid, point).ConfigureAwait(false);
            }
        }

        foreach (HtmlNodeNavigator cNode in nav.Select("//section[starts-with(@class, 'section js-hidden-item')]"))
        {
            var kind = cNode.SelectSingleNode(".//h2[@class='ttl']//img/@alt")?.Value?.Trim()
                        ?? cNode.SelectSingleNode(".//h2")?.Value?.Trim();
            var period = cNode.SelectSingleNode(".//p[contains(@class, 'txt--period')]")?.Value?.Trim();

            DateOnly? start = null, end = null;

            if (period != null
                && Regex.Match(period, @"^(\d+)年(\d+)月(\d+)日（[日月火水木金土]）～") is var ma
                && ma.Success
                && int.TryParse(ma.Groups[1].Value, out var sy)
                && 1 <= sy && sy <= 9999
                && int.TryParse(ma.Groups[2].Value, out var sm)
                && 1 <= sm && sm <= 12
                && int.TryParse(ma.Groups[3].Value, out var sd)
                && 1 <= sd && sd <= DateTime.DaysInMonth(sy, sm))
            {
                start = new DateOnly(sy, sm, sd);

                var ma2 = Regex.Match(period, "～(\\d{0,4})年?(\\d+)月(\\d+)日（[日月火水木金土]）$");
                if (ma2.Success
                    && int.TryParse(ma2.Groups[2].Value, out var em)
                    && 1 <= em && em <= 12
                    && int.TryParse(ma2.Groups[3].Value, out var ed)
                    && 1 <= ed)
                {
                    var ey = ma2.Groups[1] is var g1 && g1.Length == 0 ? sy : int.Parse(g1.Value);

                    if (1 <= ey && ey <= 9999 && ed <= DateTime.DaysInMonth(ey, em))
                    {
                        end = new DateOnly(ey, em, ed);
                    }
                }
            }

            foreach (HtmlNodeNavigator iNode in cNode.Select(".//div[@class='grid__item']"))
            {
                var item = iNode.SelectSingleNode(".//p[@class='item__txt']")?.Value?.Trim()
                        ?? iNode.SelectSingleNode(".//a[@data-modal]/@data-name")?.Value?.Trim();

                var c = newCoordinates.FirstOrDefault(e => e.Name == item);

                if (c != null)
                {
                    c.Kind = kind;
                    c.Start = start;
                    c.End = end;
                }
            }
        }

        foreach (var oc in oldCoordinateItems)
        {
            if (!newCoordinateItems.Contains(oc))
            {
                d.DataSet.CoordinateItems.Remove(oc);
            }
        }

        if (chapter != null)
        {
            chapter.Start = newCoordinates.Min(e => e.Start);
            chapter.End = newCoordinates.Max(e => e.End);
        }
    }

    static async Task ParseCardAsync(DownloadContext d, string itemUrl, bool parseChapters)
    {
        var url = new Uri(itemUrl);
        using var html = await d.GetAsync(itemUrl).ConfigureAwait(false);

        var doc = new HtmlDocument();
        doc.Load(html);

        var sn = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

        var chapterKey = Path.GetFileNameWithoutExtension(itemUrl);
        Chapter? chapter = d.DataSet.HimitsuChapters.FirstOrDefault(e => e.Id == chapterKey);

        if (!string.IsNullOrEmpty(sn)
            && Regex.Match(sn, "^(【(?<coord>[^】]+)】|(?<coord>.+))カードリスト") is var m
            && m.Success)
        {
            var ck = Path.GetFileNameWithoutExtension(itemUrl);
            var cn = m.Groups["coord"].Value;

            chapter = d.AddHimitsuChapter(ck, cn);
            Console.WriteLine(" - HimitsuChapter[{0}]: {1}", ck, cn);
        }

        var nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        if (parseChapters)
        {
            foreach (HtmlNodeNavigator cNode in nav.Select("//ul[@class='toggleList toggleList--pageSelect']/li/a"))
            {
                var v = Path.GetFileNameWithoutExtension(cNode.GetAttribute("href", null));
                var t = cNode.Value?.Trim();
                d.AddHimitsuChapter(v, t ?? v);
                Console.WriteLine("   - HimitsuChapter[{0}]: {1}", v, t ?? v);
            }
        }

        nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        var oldCards = d.DataSet.Cards.Where(e => e.ChapterId == chapter?.Id).ToList();

        var newCards = new HashSet<Card>();

        foreach (HtmlNodeNavigator cNode in nav.Select("//div[@class='grid__item']//a[@data-modal]"))
        {
            var term = cNode.GetAttribute("data-term", null);
            var name = cNode.GetAttribute("data-name", null);
            var img1 = cNode.GetAttribute("data-img1", null);
            var img2 = cNode.GetAttribute("data-img2", null);

            if (string.IsNullOrEmpty(term)
                || string.IsNullOrEmpty(name)
                || string.IsNullOrEmpty(img1))
            {
                continue;
            }

            var sealId = Path.GetFileNameWithoutExtension(img1);
            if (Regex.IsMatch(sealId, @"\d\d\d_[OU]"))
            {
                sealId = sealId[..^2];
            }

            var card = await d.AddCardAsync(
                chapter,
                name,
                term,
                sealId,
                string.IsNullOrEmpty(img1) ? null : new Uri(url, img1).ToString(),
                string.IsNullOrEmpty(img2) ? null : new Uri(url, img2).ToString()).ConfigureAwait(false);

            if (newCards.Add(card))
            {
                Console.WriteLine("   - Card[{0}]: {1}@{2}", card.Id, card.Coordinate, card.Character);
            }
        }

        foreach (var oc in oldCards)
        {
            if (!newCards.Contains(oc))
            {
                d.DataSet.Cards.Remove(oc);
            }
        }
    }

    private static async Task CorrectKnownTypoAsync(DownloadContext d)
    {
        var cor = new FileInfo(Path.Combine(DownloadContext.GetDirectory(), "correction.json"));
        if (cor.Exists)
        {
            using var fs = cor.OpenRead();
            var cd = await JsonSerializer.DeserializeAsync<CorrectionData>(fs).ConfigureAwait(false);

            foreach (var ci in cd?.Coordinates ?? [])
            {
                var dt = ci?.Data;
                if (dt != null)
                {
                    var pred = ci!.Key switch
                    {
                        nameof(dt.Id)
                        or "" or null => (Func<Coordinate, bool>)(e => e.Id == dt.Id),
                        _ => throw new ArgumentException()
                    };

                    var t = d.DataSet.Coordinates.FirstOrDefault(pred);
                    if (t == null && dt.Id > 0)
                    {
                        t = new() { Id = dt.Id };
                        d.DataSet.Coordinates.Add(t);
                    }
                    if (t != null)
                    {
                        t.ChapterId = dt.ChapterId ?? t.ChapterId;
                        t.Star = dt.Star ?? t.Star;
                        t.Name = dt.Name ?? t.Name;
                        t.Kind = dt.Kind ?? t.Kind;
                    }
                }
            }
            foreach (var ci in cd?.CoordinateItems ?? [])
            {
                var dt = ci?.Data;
                if (dt != null)
                {
                    var pred = ci!.Key switch
                    {
                        nameof(dt.Id)
                        or "" or null => (Func<CoordinateItem, bool>)(e => e.Id == dt.Id),
                        _ => throw new ArgumentException()
                    };

                    var t = d.DataSet.CoordinateItems.FirstOrDefault(pred);
                    if (t == null && dt.Id > 0 && d.DataSet.Coordinates.GetById(dt.CoordinateId) is Coordinate coord)
                    {
                        t = new()
                        {
                            Id = dt.Id,
                            CoordinateId = coord.Id
                        };
                        d.DataSet.CoordinateItems.Add(t);
                    }
                    if (t != null)
                    {
                        t.SealId = dt.SealId ?? t.SealId;
                        t.Term = dt.Term ?? t.Term;
                        t.Point = dt.Point > 0 ? dt.Point : t.Point;
                        if (!string.IsNullOrEmpty(dt.ImageUrl))
                        {
                            t.ImageUrl = await d.GetOrCopyImageAsync(dt.ImageUrl, "coordinateItems", t.Id);
                        }
                    }
                }
            }
            foreach (var ci in cd?.Cards ?? [])
            {
                var dt = ci?.Data;
                if (dt != null)
                {
                    var pred = ci!.Key switch
                    {
                        nameof(dt.Id)
                        or "" or null => (Func<Card, bool>)(e => e.Id == dt.Id),
                        _ => throw new ArgumentException()
                    };

                    var t = d.DataSet.Cards.FirstOrDefault(pred);
                    if (t == null && dt.Id > 0)
                    {
                        t = new() { Id = dt.Id };
                        d.DataSet.Cards.Add(t);
                    }
                    if (t != null)
                    {
                        t.SealId = dt.SealId.TrimOrNull() ?? t.SealId;
                        t.ChapterId = dt.ChapterId.TrimOrNull() ?? t.ChapterId;
                        t.Coordinate = dt.Coordinate.TrimOrNull() ?? t.Coordinate;
                        t.Character = dt.Character.TrimOrNull() ?? t.Character;
                        t.Variant = dt.Variant.TrimOrNull() ?? t.Variant;

                        if (!string.IsNullOrEmpty(dt.Image1Url))
                        {
                            t.Image1Url = await d.GetOrCopyImageAsync(dt.Image1Url, "cards", t.Id, "-1");
                        }
                        if (!string.IsNullOrEmpty(dt.Image2Url))
                        {
                            t.Image2Url = await d.GetOrCopyImageAsync(dt.Image2Url, "cards", t.Id, "-2");
                        }
                        Console.WriteLine($"Corrected card info of #{t.Id}({t.SealId})");
                    }
                }
            }
        }
    }

    private static async Task AddCardInfoAsync(DownloadContext d)
    {
        var cor = new FileInfo(Path.Combine(DownloadContext.GetDirectory(), "Cards.csv"));
        if (cor.Exists)
        {
            using var fs = cor.OpenRead();
            using var sr = new StreamReader(fs, Encoding.GetEncoding(932));

            var header = await sr.ReadLineAsync().ConfigureAwait(false);

            if (header != null)
            {
                var ha = header.Split(',');
                var sealId = Array.IndexOf(ha, "SealId");
                var song = Array.IndexOf(ha, "Song");
                var point = Array.IndexOf(ha, "Point");
                var star = Array.IndexOf(ha, "Star");
                var isChance = Array.IndexOf(ha, "IsChance");
                var brand = Array.IndexOf(ha, "Brand");

                if (sealId < 0)
                {
                    return;
                }

                var brands = d.DataSet.Brands.ToDictionary(e => e.Name, e => e.Id);

                for (var l = await sr.ReadLineAsync().ConfigureAwait(false); l != null; l = await sr.ReadLineAsync().ConfigureAwait(false))
                {
                    var row = l.Split(',');

                    var sid = row.ElementAtOrDefault(sealId);
                    if (string.IsNullOrEmpty(sid))
                    {
                        continue;
                    }

                    var elem = d.DataSet.Cards.FirstOrDefault(e => e.SealId == sid);

                    if (elem == null)
                    {
                        continue;
                    }

                    Console.WriteLine("Set card info of " + sid);

                    string? read(int id)
                        => id >= 0 ? row.ElementAt(id).TrimOrNull() : null;

                    elem.Song = read(song) ?? string.Empty;
                    elem.Point = short.TryParse(read(point), out var s) ? s : default;
                    elem.Star = byte.TryParse(read(star), out var st) ? st : (byte)0;
                    elem.IsChance = bool.TryParse(read(isChance), out var b) && b;
                    elem.BrandId = brands.TryGetValue(read(brand) ?? string.Empty, out var bid) ? bid : null;
                }
            }
        }
    }
}