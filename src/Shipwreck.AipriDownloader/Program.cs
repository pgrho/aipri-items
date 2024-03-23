using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Shipwreck.AipriDownloader;

internal class Program
{
    private const string VERSE_HOME = "https://aipri.jp/verse/";

    static async Task Main(string[] args)
    {
        using var d = new DownloadContext();

        using var html = await d.GetAsync(VERSE_HOME).ConfigureAwait(false);

        var doc = new HtmlDocument();
        doc.Load(html);

        foreach (var a in doc.DocumentNode.SelectNodes("//a[@class='globalNav__link']"))
        {
            var href = a.GetAttributeValue("href", null);
            if (href != null && Regex.IsMatch(href, @"^\/verse\/item\/(\d)\.html$"))
            {
                await ParseItemAsync(d, GetAbsoluteUrl(href)).ConfigureAwait(false);
                break;
            }
        }

        // 既知の誤記載を訂正する
        await CorrectKnownTypoAsync(d).ConfigureAwait(false);
    }

    [return: NotNullIfNotNull(nameof(path))]
    internal static string? GetAbsoluteUrl(string? path)
        => string.IsNullOrEmpty(path) ? null : new Uri(new Uri(VERSE_HOME), path).ToString();

    static async Task ParseItemAsync(DownloadContext d, string itemUrl)
    {
        using var html = await d.GetAsync(itemUrl).ConfigureAwait(false);

        var doc = new HtmlDocument();
        doc.Load(html);

        var sn = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

        Chapter? chapter = null;

        if (!string.IsNullOrEmpty(sn)
            && Regex.Match(sn, "^【([^】]+)】") is var m
            && m.Success)
        {
            var ck = Path.GetFileNameWithoutExtension(itemUrl);
            var cn = m.Groups[1].Value;

            chapter = d.AddChapter(ck, cn);
            Console.WriteLine(" - Chapter[{0}]: {1}", ck, cn);
        }

        var nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        var currentCoords = new List<Coordinate>();
        foreach (HtmlNodeNavigator cNode in nav.Select("//div[starts-with(@class, 'modal__item')]"))
        {
            var star = cNode.SelectSingleNode(".//p[@class='item__star']/img/@alt")?.Value?.Trim();
            var title = cNode.SelectSingleNode(".//p[@class='item__ttl']")?.Value?.Trim();
            var imgUrl = cNode.SelectSingleNode(".//p[@class='item__img']/img/@src")?.Value?.Trim();
            var brand = cNode.SelectSingleNode(".//p[@class='item__brand']/img");
            var bndImg = brand?.SelectSingleNode("@src")?.Value?.Trim();
            var bndName = brand?.SelectSingleNode("@alt")?.Value?.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(star))
            {
                continue;
            }

            var b = string.IsNullOrEmpty(bndName) ? null
                : await d.AddBrandAsync(bndName, GetAbsoluteUrl(bndImg)).ConfigureAwait(false);

            var coord = await d.AddCoordinateAsync(chapter, b, title, star, GetAbsoluteUrl(imgUrl)).ConfigureAwait(false);

            currentCoords.Add(coord);

            foreach (HtmlNodeNavigator iNode in cNode.Select(".//dl[@class='itemList__item']"))
            {
                var term = iNode.SelectSingleNode(".//dt[@class='itemList__term']")?.Value?.Trim();
                var iUrl = iNode.SelectSingleNode(".//dd[@class='itemList__desc']/img/@src")?.Value?.Trim();
                var iid = iNode.SelectSingleNode(".//p[@class='item__id']")?.Value?.Trim();
                var point = short.TryParse(iNode.SelectSingleNode(".//p[@class='item__point']")?.Value?.Trim(), out var pv) ? pv : (short)0;

                if (string.IsNullOrEmpty(iid) || string.IsNullOrEmpty(term))
                {
                    continue;
                }

                var eid = iUrl != null && Regex.Match(iUrl, "\\/Item_ID(\\d+)\\.webp$") is var em && em.Success ? int.Parse(em.Groups[1].Value)
                    : (int?)null;

                await d.AddItemAsync(coord, iid, term, point, GetAbsoluteUrl(iUrl), eid).ConfigureAwait(false);
            }
        }

        foreach (HtmlNodeNavigator cNode in nav.Select("//section[starts-with(@class, 'section js-hidden-item')]"))
        {
            var kind = cNode.SelectSingleNode(".//h2[@class='ttl']//img/@alt")?.Value?.Trim();
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

            foreach (HtmlNodeNavigator iNode in cNode.Select(".//div[@class='grid__item modal__hiddenItem']"))
            {
                var iUrl = GetAbsoluteUrl(iNode.SelectSingleNode(".//img/@src")?.Value?.Trim());
                var item = iNode.SelectSingleNode(".//p[@class='item__txt']")?.Value?.Trim();

                var c = currentCoords.FirstOrDefault(e => e.Name == item);

                if (c != null)
                {
                    c.Kind = kind;
                    c.Start = start;
                    c.End = end;

                    if (c.ThumbnailUrl != iUrl || !c.IsThumbnailLoaded)
                    {
                        c.ThumbnailUrl = await d.GetOrCopyImageAsync(iUrl, "coordinate-thumbnails", c.Id).ConfigureAwait(false);
                        c.IsThumbnailLoaded = true;
                    }
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