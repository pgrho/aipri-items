using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Shipwreck.AipriDownloader;

internal class Program
{
    private const string VERSE_HOME = "https://aipri.jp/verse/";
    private const string ITEM_FORMAT = VERSE_HOME + "item/{0}.html";

    static async Task Main(string[] args)
    {
        using var d = new DownloadContext();

        await ParseItemAsync(d, string.Format(ITEM_FORMAT, "1"), true).ConfigureAwait(false);

        foreach (var ch in d.DataSet.Chapters.Where(e => e.Id != "1"))
        {
            await ParseItemAsync(d, string.Format(ITEM_FORMAT, ch.Id), false).ConfigureAwait(false);
        }

        // 既知の誤記載を訂正する
        await CorrectKnownTypoAsync(d).ConfigureAwait(false);
    }

    static async Task ParseItemAsync(DownloadContext d, string itemUrl, bool parseChapters)
    {
        var url = new Uri(itemUrl);
        using var html = await d.GetAsync(itemUrl).ConfigureAwait(false);

        var doc = new HtmlDocument();
        doc.Load(html);

        var sn = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

        var chapterKey = Path.GetFileNameWithoutExtension(itemUrl);
        Chapter? chapter = d.DataSet.Chapters.FirstOrDefault(e => e.Id == chapterKey);

        if (!string.IsNullOrEmpty(sn)
            && Regex.Match(sn, "^(【(?<coord>[^】]+)】|(?<coord>.+))コーデアイテム") is var m
            && m.Success)
        {
            var ck = Path.GetFileNameWithoutExtension(itemUrl);
            var cn = m.Groups["coord"].Value;

            chapter = d.AddChapter(ck, cn);
            Console.WriteLine(" - Chapter[{0}]: {1}", ck, cn);
        }

        var nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        if (parseChapters)
        {
            foreach (HtmlNodeNavigator cNode in nav.Select("//ul[@class='toggleList toggleList--pageSelect']/li/a/@href"))
            {
                var v = Path.GetFileNameWithoutExtension(cNode.Value);
                d.AddChapter(v, v);
                Console.WriteLine(" - Chapter[{0}]: {1}", v, v);
            }
        }

        nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();
        var currentCoords = new HashSet<Coordinate>();
        foreach (HtmlNodeNavigator cNode in nav.Select("//div[@class='grid__item']//a[@data-modal]"))
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

            if (modal == "item")
            {
                var imgUrl = cNode.SelectSingleNode("@data-img")?.Value;

                if (string.IsNullOrEmpty(title))
                {
                    continue;
                }

                var coord = await d.AddCoordinateAsync(chapter, b, title, star, new Uri(url, imgUrl).ToString()).ConfigureAwait(false);

                currentCoords.Add(coord);

                var thUrl = cNode.SelectSingleNode("./img/@src")?.Value;
                if (thUrl != null)
                {
                    thUrl = new Uri(url, thUrl).ToString();
                    if (coord.ThumbnailUrl != thUrl || !coord.IsThumbnailLoaded)
                    {
                        coord.ThumbnailUrl = await d.GetOrCopyImageAsync(thUrl, "coordinate-thumbnails", coord.Id).ConfigureAwait(false);
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

                    var eid = iUrl != null && Regex.Match(iUrl, "\\/Item_ID(\\d+)\\.webp$") is var em && em.Success ? int.Parse(em.Groups[1].Value)
                        : (int?)null;

                    await d.AddItemAsync(coord, iid, term, point, new Uri(url, iUrl).ToString(), eid).ConfigureAwait(false);
                }
            }
            else if (modal == "special")
            {
                if (string.IsNullOrEmpty(title))
                {
                    continue;
                }

                var coord = await d.AddCoordinateAsync(chapter, b, title, star, null).ConfigureAwait(false);

                currentCoords.Add(coord);

                var term = cNode.SelectSingleNode("@data-term")?.Value?.Trim();
                if (string.IsNullOrEmpty(term))
                {
                    break;
                }
                var iUrl = cNode.SelectSingleNode("@data-img")?.Value?.Trim();
                var iid = cNode.SelectSingleNode("@data-id")?.Value?.Trim();
                var point = short.TryParse(cNode.SelectSingleNode("@data-point")?.Value?.Trim(), out var pv) ? pv : (short)0;

                var eid = iUrl != null && Regex.Match(iUrl, "\\/Item_ID(\\d+)\\.webp$") is var em && em.Success ? int.Parse(em.Groups[1].Value)
                    : (int?)null;

                await d.AddItemAsync(coord, iid, term, point, new Uri(url, iUrl).ToString(), eid).ConfigureAwait(false);
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

                var c = currentCoords.FirstOrDefault(e => e.Name == item);

                if (c != null)
                {
                    c.Kind = kind;
                    c.Start = start;
                    c.End = end;
                }
            }
        }

        // TODO 現在のチャプターで記載のないコーデを削除する

        if (chapter != null)
        {
            chapter.Start = currentCoords.Min(e => e.Start);
            chapter.End = currentCoords.Max(e => e.End);
        }
    }

    private static async Task CorrectKnownTypoAsync(DownloadContext d)
    {
        var cor = new FileInfo(Path.Combine(DownloadContext.GetDirectory(), "correction.json"));
        if (cor.Exists)
        {
            using var fs = cor.OpenRead();
            var cd = await JsonSerializer.DeserializeAsync<CorrectionData>(fs).ConfigureAwait(false);

            foreach (var ci in cd?.CoordinateItems ?? [])
            {
                if (ci?.Data != null)
                {
                    var pred = ci.Key switch
                    {
                        nameof(ci.Data.Id) => (Func<CoordinateItem, bool>)(e => e.Id == ci.Data.Id),
                        _ => throw new ArgumentException()
                    };

                    var t = d.DataSet.CoordinateItems.FirstOrDefault(pred);
                    if (t != null)
                    {
                        t.SealId = ci.Data.SealId.TrimOrNull() ?? t.SealId;
                        t.Term = ci.Data.Term.TrimOrNull() ?? t.Term;
                        t.Point = ci.Data.Point.PositiveOrNull() ?? t.Point;
                    }
                }
            }
        }
    }
}