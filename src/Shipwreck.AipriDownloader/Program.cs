using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Sources;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using Shipwreck.Aipri;
using Shipwreck.Phash;
using Shipwreck.Phash.PresentationCore;

namespace Shipwreck.AipriDownloader;

internal class Program
{
    private const string VERSE_HOME = "https://aipri.jp/verse/";
    private const string ITEM_FORMAT = VERSE_HOME + "item/{0}.html";
    private const string PARTS = VERSE_HOME + "parts/";

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

        await ParsePartsAsync(d).ConfigureAwait(false);

        var order = 1;
        await ParseItemAsync(d, string.Format(ITEM_FORMAT, "1"), true, order).ConfigureAwait(false);

        foreach (var ch in d.DataSet.VerseChapters.Where(e => e.Id != "1"))
        {
            order += 1000;
            await ParseItemAsync(d, string.Format(ITEM_FORMAT, ch.Id), false, order).ConfigureAwait(false);
        }

        await Task.WhenAll(
                    d.DataSet.Coordinates.SelectMany(p => new[]
                    {
                        p.ImageTask,
                        p.ThumbnailTask,
                    }).Concat(d.DataSet.CoordinateItems.Select(e => e.ImageTask)).OfType<Task>())
                .ConfigureAwait(false);

        order = 1;
        await ParseCardAsync(d, string.Format(CARD_FORMAT, "1"), true, order).ConfigureAwait(false);

        foreach (var ch in d.DataSet.HimitsuChapters.Where(e => e.Id != "1"))
        {
            order += 1000;
            await ParseCardAsync(d, string.Format(CARD_FORMAT, ch.Id), false, order).ConfigureAwait(false);
        }

        await Task.WhenAll(
            d.DataSet.Cards.SelectMany(e => new[]
            {
                e.Image1Task,
                e.Image2Task
            }).OfType<Task>()).ConfigureAwait(false);

        var dupBrands = d.DataSet.Brands.GroupBy(e => e.Name).SelectMany(g =>
        {
            var l = g.OrderBy(e => e.Id).ToList();
            return l.Skip(1).Select(e => (first: l[0], item: e));
        }).ToDictionary(e => e.item.Id, e => e.first);

        foreach (var c in d.DataSet.Cards)
        {
            if (c.BrandId is int did
                && dupBrands.TryGetValue(did, out var b))
            {
                c.BrandId = b.Id;
            }
        }
        foreach (var c in d.DataSet.Coordinates)
        {
            if (c.BrandId is int did
                && dupBrands.TryGetValue(did, out var b))
            {
                c.BrandId = b.Id;
            }
        }

        // 既知の誤記載を訂正する
        await FixCoordinateInfoAsync(d).ConfigureAwait(false);
        await FixCoordinateItemInfoAsync(d).ConfigureAwait(false);
        await FixCardInfoAsync(d).ConfigureAwait(false);

        foreach (var chCor in await d.EnumerateCharacterCorrection().ConfigureAwait(false))
        {
            var ch = d.DataSet.Characters.FirstOrDefault(e => e.Name == chCor.Name);
            if (ch != null
                && !string.IsNullOrEmpty(chCor.ShortName)
                && chCor.ShortName != chCor.Name)
            {
                ch.ShortName = chCor.ShortName;
            }
        }

        var used = d.DataSet.Cards.Select(e => e.BrandId ?? 0).Concat(d.DataSet.Coordinates.Select(e => e.BrandId ?? 0)).Where(e => e > 0).Distinct().ToList();

        foreach (var b in d.DataSet.Brands.Where(e => !used.Contains(e.Id)).ToList())
        {
            d.DataSet.Brands.Remove(b);
        }

        var comparer = StringComparer.Create(
            CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

        foreach (var c in d.DataSet.HimitsuChapters)
        {
            var vc = d.DataSet.VerseChapters.FirstOrDefault(e => e.Id == c.Id);
            c.Start ??= vc?.Start;
        }

        var chanceCoords = d.DataSet.Cards.Where(e => e.IsChance).GroupBy(e => e.Coordinate, comparer).ToDictionary(e => e.Key, g => g.Min(e => e.GetChapter()?.Start), comparer);
        foreach (var c in d.DataSet.Coordinates)
        {
            c.HasChance = chanceCoords.TryGetValue(c.Name, out var cardStart);
            c.Start ??= cardStart;
        }

        var songHashes = new Dictionary<Song, (ulong dct, byte[] coefficients)>();
        foreach (var sg in d.DataSet.Cards.GroupBy(e => e.GetSong()))
        {
            if (sg.Key is not Song song)
            {
                continue;
            }

            song.SingerIds = sg.Select(e => e.CharacterId).Where(e => e > 0).Distinct().Order().ToList();

            var img = new FileInfo(Path.Combine(d.GetCustomDirectory(), "Songs", song.Name + ".jpg"));
            if (img.Exists)
            {
                using var stream = img.OpenRead();
                var bmp = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);

                var bytes = bmp.ToLuminanceImage();

                songHashes.Add(song, (ImagePhash.ComputeDctHash(bytes), ImagePhash.ComputeDigest(bytes).Coefficients));
            }
        }

        foreach (var c in d.DataSet.Cards
                                .Where(e => e.GetSong() is not Song s || !songHashes.ContainsKey(s))
                                .Where(e => e.Image2Url?.StartsWith("https://aipri.jp/") == true
                                            && e.Image2Url.EndsWith(".webp")))
        {
            var img = new FileInfo(Path.Combine(Path.GetDirectoryName(d.GetCustomDirectory())!, "output", string.Format(Constants.CARD_PATH_FORMAT2, c.Id, ".webp")));

            if (!img.Exists)
            {
                continue;
            }

            using var stream = img.OpenRead();
            BitmapSource bmp = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            const int W = 814;
            const int H = 1186;

            if (bmp.PixelWidth != W || bmp.PixelHeight != H)
            {
                bmp = new TransformedBitmap(bmp, new ScaleTransform(W / (double)bmp.PixelWidth, H / (double)bmp.PixelHeight));
            }

            var s = c.GetSong();
            if (s == null || !songHashes.ContainsKey(s))
            {
                var songImage = new CroppedBitmap(bmp, new(546, 349, 250, 250));

                var bytes = songImage.ToLuminanceImage();

                var dct = ImagePhash.ComputeDctHash(bytes);
                var coeff = ImagePhash.ComputeDigest(bytes).Coefficients;

                if (s != null)
                {
                    var enc = new JpegBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(songImage));
                    using var fs = new FileStream(Path.Combine(d.GetCustomDirectory(), "Songs", s.Name + ".jpg"), FileMode.Create);
                    enc.Save(fs);

                    songHashes.Add(s, (dct, coeff));
                }
                else
                {
                    var matched = songHashes.Select(
                        e => (
                        s: e.Key,
                        r: ImagePhash.GetHammingDistance(e.Value.dct ^ dct) >= 8 ? -1 : ImagePhash.GetCrossCorrelation(e.Value.coefficients, coeff)))
                        .Where(e => e.r > 0.9)
                        .OrderByDescending(e => e.r)
                        .FirstOrDefault().s;

                    c.Song = matched?.Name;
                    c.SongId = matched?.Id ?? 0;
                }
            }
        }
    }

    static async Task ParseItemAsync(DownloadContext d, string itemUrl, bool parseChapters, int startOrder)
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
                var t = cNode.Value?.Trim2();
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

        var tags = new Dictionary<string, string>();

        foreach (HtmlNodeNavigator section in nav.Select("//li[starts-with(@class, 'tagList__item')]/a"))
        {
            var key = section.GetAttribute("data-show", null)?.Trim2();
            var value = section.Value?.Trim2();

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                tags[key] = value;
            }
        }

        foreach (HtmlNodeNavigator section in nav.Select("//section[starts-with(@class, 'section js-hidden-item')]"))
        {
            var kind = section.SelectSingleNode(".//h2[@class='ttl']//img/@alt")?.Value?.Trim2()
                        ?? section.SelectSingleNode(".//h2")?.Value?.Trim2();

            var group = kind?.StartsWith("ひみつのアイプリファンブック") == true ? "ひみつのアイプリファンブック"
                : kind?.StartsWith("ひみつのアイプリブレス") == true ? "ひみつのアイプリブレス"
                : kind?.StartsWith("ひみつのアイプリリップ") == true ? "ひみつのアイプリリップ"
                : kind?.StartsWith("ひみつのアイプリ") == true && kind?.EndsWith("コスメセット") == true ? "ひみつのアイプリコスメセット"
                : kind?.StartsWith("ひみつのアイプリ ぬいぐるみ") == true ? "ぬいぐるみ・マスコット"
                : kind?.StartsWith("ひみつのアイプリ ボールチェーンマスコット") == true ? "ぬいぐるみ・マスコット"
                : kind?.StartsWith("ちゃお") == true ? "ちゃお付録"
                : kind?.StartsWith("お店でアイプリグランプリ") == true ? "店頭大会"
                : kind;

            foreach (HtmlNodeNavigator cNode in section.Select(".//div[@class='grid__item' or starts-with(@class, 'grid__item ')]//a[@data-modal]"))
            {
                var period = (cNode.Select("../../preceding-sibling::p[contains(@class, 'txt--period')]").OfType<HtmlNodeNavigator>().LastOrDefault()
                            ?? section.SelectSingleNode(".//p[contains(@class, 'txt--period')]"))?.Value?.Trim2();

                DateOnly? start = null, end = null;

                if (period != null
                    && Regex.Match(period, @"^(\d+)年(\d+)月(\d+)日[（(][日月火水木金土][）)]～") is var ma
                    && ma.Success
                    && int.TryParse(ma.Groups[1].Value, out var sy)
                    && 1 <= sy && sy <= 9999
                    && int.TryParse(ma.Groups[2].Value, out var sm)
                    && 1 <= sm && sm <= 12
                    && int.TryParse(ma.Groups[3].Value, out var sd)
                    && 1 <= sd && sd <= DateTime.DaysInMonth(sy, sm))
                {
                    start = new DateOnly(sy, sm, sd);

                    var ma2 = Regex.Match(period, "～(\\d{0,4}?)年?(\\d+)月(\\d+)日[(（][日月火水木金土][）)]$");
                    if (ma2.Success
                        && int.TryParse(ma2.Groups[2].Value, out var em)
                        && 1 <= em && em <= 12
                        && int.TryParse(ma2.Groups[3].Value, out var ed)
                        && 1 <= ed)
                    {
                        var ey = ma2.Groups[1] is var g1
                                && g1.Length > 0 ? int.Parse(g1.Value)
                                : em < sm ? sy + 1
                                : sy;

                        if (1 <= ey && ey <= 9999 && ed <= DateTime.DaysInMonth(ey, em))
                        {
                            end = new DateOnly(ey, em, ed);
                        }
                    }
                }

                var modal = cNode.SelectSingleNode("@data-modal")?.Value;

                var bndImg = cNode.SelectSingleNode("@data-brand")?.Value;
                var bndName = cNode.SelectSingleNode("@data-brandname")?.Value;

                var b = string.IsNullOrEmpty(bndName) ? null
                    : await d.AddBrandAsync(bndName, string.IsNullOrEmpty(bndImg) ? null : new Uri(url, bndImg).ToString()).ConfigureAwait(false);

                var title = cNode.SelectSingleNode("@data-name")?.Value?.Replace("<br>", "")?.Replace("\u3000", " ");

                var star = cNode.SelectSingleNode("@data-icn")?.Value is string icn
                        && Regex.Match(Path.GetFileNameWithoutExtension(icn), "^icn_star\\d$") is var icnM
                        && icnM.Success
                        ? icnM.Value.Last() - '0'
                        : (int?)null;

                async Task addItemAsync(Coordinate coord, string? category, string? iUrl, string? iid, short point)
                {
                    coord.Group = (chapterKey == "special" ? group : null) ?? kind;
                    coord.Kind = kind;
                    coord.Start = start;
                    coord.End = end;
                    var eid = iUrl != null && Regex.Match(iUrl, "\\/Item_ID(\\d+)\\.webp$") is var em && em.Success ? int.Parse(em.Groups[1].Value)
                        : (int?)null;

                    var first = (string.IsNullOrEmpty(iid)
                        ? null
                        : d.DataSet.CoordinateItems.FirstOrDefault(e => e.SealId == iid && e.IsCurrentRun))
                        ?? (d.DataSet.CoordinateItems.FirstOrDefault(e => e.GetCoordinate()?.Name == coord.Name && e.GetCategory()?.Name == category));

                    if (first == null || first.CoordinateId == coord.Id)
                    {
                        var item = await d.AddItemAsync(coord, iid, category, point, new Uri(url, iUrl).ToString(), eid).ConfigureAwait(false);

                        item.IsCurrentRun = true;

                        Console.WriteLine("     - Item[{0}]: {1}", item.Id, category);

                        newCoordinateItems.Add(item);
                    }
                    else
                    {
                        coord.LinkedItemIds.Add(first.Id);
                        Console.WriteLine("     - Item[{0}]: {1} (Linked to {2})", first.Id, first.GetCategory()?.Name, d.DataSet.Coordinates.GetById(first.CoordinateId)?.Name);
                    }
                }

                if (modal == "item")
                {
                    var imgUrl = cNode.SelectSingleNode("@data-img")?.Value;

                    if (string.IsNullOrEmpty(title))
                    {
                        continue;
                    }

                    var coord = d.AddCoordinate(chapter, b, kind, title, star, new Uri(url, imgUrl).ToString());

                    if (newCoordinates.Add(coord))
                    {
                        Console.WriteLine("   - Coordinate[{0}]: {1}", coord.Id, coord.Name);
                    }
                    coord.Order = startOrder++;

                    var thUrl = cNode.SelectSingleNode("./img/@src")?.Value;
                    if (thUrl != null)
                    {
                        thUrl = new Uri(url, thUrl).ToString();
                        coord.BeginSetThumbnailUrl(thUrl, d);
                    }

                    for (var n = 1; n <= 4; n++)
                    {
                        var category = cNode.SelectSingleNode("@data-term" + n)?.Value?.Trim2()?.Replace("ヘアアクセ", "アクセ");
                        if (string.IsNullOrEmpty(category))
                        {
                            break;
                        }
                        var iUrl = cNode.SelectSingleNode("@data-img" + n)?.Value?.Trim2();
                        var iid = cNode.SelectSingleNode("@data-id" + n)?.Value?.Trim2();
                        var point = short.TryParse(cNode.SelectSingleNode("@data-point" + n)?.Value?.Trim2(), out var pv) ? pv : (short)0;

                        await addItemAsync(coord, category, iUrl, iid, point).ConfigureAwait(false);
                    }
                }
                else if (modal == "special")
                {
                    if (string.IsNullOrEmpty(title))
                    {
                        continue;
                    }

                    var coord = d.AddCoordinate(chapter, b, kind, title, star, null);

                    if (newCoordinates.Add(coord))
                    {
                        Console.WriteLine("   - Coordinate[{0}]: {1}", coord.Id, coord.Name);
                    }
                    coord.Order = startOrder++;

                    var category = cNode.SelectSingleNode("@data-term")?.Value?.Trim2();
                    if (string.IsNullOrEmpty(category))
                    {
                        break;
                    }
                    var iUrl = cNode.SelectSingleNode("@data-img")?.Value?.Trim2();
                    var iid = cNode.SelectSingleNode("@data-id")?.Value?.Trim2();
                    var point = short.TryParse(cNode.SelectSingleNode("@data-point")?.Value?.Trim2(), out var pv) ? pv : (short)0;

                    await addItemAsync(coord, category, iUrl, iid, point).ConfigureAwait(false);
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
            //var re = new Regex("^リング[1-9１-９]だん(前|後)半$");
            //foreach (var kg in newCoordinates.GroupBy(e => e.Kind))
            //{
            //    if (kg.Key != null && re.IsMatch(kg.Key))
            //    {
            //        var suf = kg.Key[^2];

            //        if (!newCoordinates.Any(e => e.Kind?.Contains(suf) == true
            //                                && e.Kind?.EndsWith("ピックアップ") == true))
            //        {
            //            var nt = kg.Key + "★★★★ピックアップ";

            //            foreach (var e in kg)
            //            {
            //                e.Kind = nt;
            //            }
            //        }
            //    }
            //}
            var pickups = newCoordinates.Where(e => e.Kind?.Contains("だん") == true).Select(e => new { e.Start, e.End }).Distinct().ToList();

            var ps = pickups.Min(e => e.Start);
            var pe = pickups.Max(e => e.End);

            if (ps != null && pe != null)
            {
                chapter.Start = ps;
                chapter.End = pe;

                foreach (var c in newCoordinates)
                {
                    c.Start ??= ps;
                    c.End ??= pe;
                }
            }
        }
    }

    static async Task ParsePartsAsync(DownloadContext d)
    {
        var url = new Uri(PARTS);
        using var html = await d.GetAsync(PARTS).ConfigureAwait(false);

        var doc = new HtmlDocument();
        doc.Load(html);

        var nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        var old = d.DataSet.Parts.ToList();

        var list = new HashSet<Part>();

        foreach (HtmlNodeNavigator section in nav.Select("//section[starts-with(@class, 'section ')]"))
        {
            var categoryName = section.SelectSingleNode(".//h2[@class='ttl']//img/@alt")?.Value?.Trim2();

            if (string.IsNullOrEmpty(categoryName))
            {
                continue;
            }

            var category = d.AddPartCategory(categoryName);

            foreach (HtmlNodeNavigator cNode in section.Select(".//div[@class='grid__item' or starts-with(@class, 'grid__item ')]"))
            {
                var name = cNode.SelectSingleNode(".//img/@alt")?.Value?.Trim2();

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var desc = cNode.SelectSingleNode(".//p")?.Value?.Trim2();

                var p = d.AddPart(name)!;
                p.CategoryId = category!.Id;

                p.Description = desc;

                var img = cNode.SelectSingleNode(".//img/@src")?.Value?.Trim2();
                if (img != null)
                {
                    img = new Uri(url, img).ToString();
                    if (p.ImageUrl != img || !p.IsImage1Loaded)
                    {
                        p.ImageUrl = await d.GetOrCopyImageAsync(img, "parts", p.Id).ConfigureAwait(false);
                        p.IsImage1Loaded = true;
                    }
                }

                list.Add(p);
                old.Remove(p);
            }
        }

        foreach (var oc in old)
        {
            d.DataSet.Parts.Remove(oc);
        }
    }

    static async Task ParseCardAsync(DownloadContext d, string itemUrl, bool parseChapters, int startOrder)
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
                var t = cNode.Value?.Trim2();
                d.AddHimitsuChapter(v, t ?? v);
                Console.WriteLine("   - HimitsuChapter[{0}]: {1}", v, t ?? v);
            }
        }

        nav = (HtmlAgilityPack.HtmlNodeNavigator)doc.CreateNavigator();

        var oldCards = d.DataSet.Cards.Where(e => e.ChapterId == chapter?.Id).ToList();

        var newCards = new HashSet<Card>();

        foreach (HtmlNodeNavigator cNode in nav.Select(".//div[@class='grid__item' or starts-with(@class, 'grid__item ')]//a[@data-modal]"))
        {
            var term = cNode.GetAttribute("data-term", null)
                                ?.Replace("(", "（")
                                ?.Replace(")", "）");
            var name = cNode.GetAttribute("data-name", null)
                                ?.Replace("<br>", "");
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
            card.Order = startOrder++;
        }

        foreach (var oc in oldCards)
        {
            if (!newCards.Contains(oc))
            {
                d.DataSet.Cards.Remove(oc);
            }
        }
    }

    private static async Task FixCoordinateInfoAsync(DownloadContext d)
    {
        foreach (var ci in await d.EnumerateCoordinateCorrection())
        {
            var dt = ci?.Data;
            if (dt != null)
            {
                var pred = ci!.Key switch
                {
                    nameof(dt.Id)
                    or "" or null => (Func<Coordinate, bool>)(e => e.Id == dt.Id),
                    nameof(dt.Name) => (Func<Coordinate, bool>)(e => e.Name == dt.Name),
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
                    t.ChapterId = dt.ChapterId.TrimOrNull() ?? t.ChapterId;
                    var ch = d.DataSet.VerseChapters.GetById(t.ChapterId!);
                    t.BrandId = dt.BrandId ?? t.BrandId;

                    t.Star = dt.Star ?? t.Star;
                    t.Name = dt.Name.TrimOrNull() ?? t.Name;
                    t.Group = dt.Group.TrimOrNull() ?? t.Group;
                    t.Kind = dt.Kind.TrimOrNull() ?? t.Kind;
                    t.Start = dt.Start ?? t.Start ?? ch?.Start;
                    t.End = dt.End ?? t.End ?? ch?.End;
                }
            }
        }
    }

    private static async Task FixCoordinateItemInfoAsync(DownloadContext d)
    {
        foreach (var ci in await d.EnumerateCoordinateItemCorrection())
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
                    t.SealId = (dt.SealId ?? t.SealId).TrimOrNull();
                    if (dt.CategoryId > 0)
                    {
                        t.CategoryId = dt.CategoryId;
                    }
                    t.Point = dt.Point > 0 ? dt.Point : t.Point;
                    t.BeginSetImageUrl(dt.ImageUrl, d);
                }
            }
        }

        await Task.WhenAll(d.DataSet.CoordinateItems.Select(e => e.ImageTask).OfType<Task>())
                .ConfigureAwait(false);
    }

    private static async Task FixCardInfoAsync(DownloadContext d)
    {
        foreach (var ci in await d.EnumerateCardCorrection())
        {
            var src = ci?.Data;
            if (src != null)
            {
                var pred = ci!.Key switch
                {
                    nameof(src.Id) or "" or null => (Func<Card, bool>)(e => e.Id == src.Id),
                    nameof(src.SealId) => (Func<Card, bool>)(e => e.SealId == src.SealId),
                    _ => throw new ArgumentException()
                };

                var dest = d.DataSet.Cards.FirstOrDefault(pred);
                if (src.Id > 0)
                {
                    if (dest == null)
                    {
                        dest = new() { Id = src.Id };
                        d.DataSet.Cards.Add(dest);
                    }
                    else if ((ci.Key != nameof(src.Id) && !string.IsNullOrEmpty(ci.Key))
                        && src.Id > 0
                        && src.Id > dest.Id)
                    {
                        dest.Id = src.Id;
                        dest.Image1Task = null;
                        dest.Image2Task = null;
                    }
                }
                if (dest != null)
                {
                    dest.SealId = src.SealId.TrimOrNull() ?? dest.SealId;
                    dest.ChapterId = src.ChapterId.TrimOrNull() ?? dest.ChapterId;
                    if (!double.IsNaN(src.Order))
                    {
                        dest.Order = src.Order;
                    }
                    dest.Coordinate = src.Coordinate.TrimOrNull() ?? dest.Coordinate;
                    dest.Variant = src.Variant.TrimOrNull() ?? dest.Variant;
                    if (src.CharacterId > 0)
                    {
                        dest.CharacterId = src.CharacterId;
                    }
                    if (src.SongId > 0)
                    {
                        dest.SongId = src.SongId;
                    }
                    if (src.Star > 0)
                    {
                        dest.Star = src.Star;
                    }
                    if (src.Point > 0)
                    {
                        dest.Point = src.Point;
                    }
                    dest.IsChance = src.IsChance;
                    dest.BrandId = src.BrandId;

                    if (!string.IsNullOrEmpty(src.Image1Url))
                    {
                        dest.BeginSetImage1Url(src.Image1Url, d);
                    }
                    else if (dest.Image1Task == null && dest.Image1Url != null)
                    {
                        dest.BeginSetImage1Url(dest.Image1Url, d);
                    }

                    if (!string.IsNullOrEmpty(src.Image2Url))
                    {
                        dest.BeginSetImage2Url(src.Image2Url, d);
                    }
                    else if (dest.Image2Task == null && dest.Image2Url != null)
                    {
                        dest.BeginSetImage2Url(dest.Image2Url, d);
                    }

                    Console.WriteLine($"Corrected card info of #{dest.Id}({dest.SealId})");
                }
            }
        }

        await Task.WhenAll(
            d.DataSet.Cards.SelectMany(e => new[]
            {
                e.Image1Task,
                e.Image2Task
            }).OfType<Task>()).ConfigureAwait(false);
    }
}
