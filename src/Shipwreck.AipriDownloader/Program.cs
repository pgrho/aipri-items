using System.Diagnostics.CodeAnalysis;
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
    }

    [return: NotNullIfNotNull(nameof(path))]
    private static string? GetAbsoluteUrl(string? path)
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
        var items = nav.Select("//div[starts-with(@class, 'modal__item')]");

        foreach (HtmlNodeNavigator cNode in items)
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
    }
}