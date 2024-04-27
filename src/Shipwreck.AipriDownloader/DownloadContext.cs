using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Shipwreck.Aipri;

namespace Shipwreck.AipriDownloader;

public sealed class DownloadContext : IDisposable
{
    private readonly HttpClient _Http;
    private readonly AipriDataSet _DataSet;

    private readonly DirectoryInfo _OutputDirectory;

    private readonly DirectoryInfo _CacheDirectory;
    private readonly Dictionary<string, UrlCache> _Cache;
    private readonly FileStream _JsonStream;
    private int _LastId;

    class UrlCache
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Etag { get; set; }
        public DateTime? LastModified { get; set; }
        public DateTime? LastRequested { get; set; }
    }

    public DownloadContext()
    {
        _Http = new HttpClient();
        _OutputDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(GetDirectory())!)!, "output"));
        _DataSet = new();
        if (_OutputDirectory.Exists)
        {
            try
            {
                var cf = new FileInfo(Path.Combine(_OutputDirectory.FullName, "data.json"));
                if (cf.Exists)
                {
                    using var fs = cf.OpenRead();
                    var hd = JsonSerializer.Deserialize<AipriDataSet>(fs);
                    if (hd != null)
                    {
                        foreach (var c in hd.VerseChapters)
                        {
                            _DataSet.VerseChapters.Add(c.Clone());
                        }
                        foreach (var c in hd.Categories)
                        {
                            _DataSet.Categories.Add(c.Clone());
                        }
                        foreach (var c in hd.Brands)
                        {
                            _DataSet.Brands.Add(c.Clone());
                        }
                        foreach (var c in hd.Coordinates)
                        {
                            _DataSet.Coordinates.Add(c.Clone());
                        }
                        foreach (var c in hd.CoordinateItems)
                        {
                            _DataSet.CoordinateItems.Add(c.Clone());
                        }
                        foreach (var c in hd.HimitsuChapters)
                        {
                            _DataSet.HimitsuChapters.Add(c.Clone());
                        }
                        foreach (var c in hd.Songs)
                        {
                            _DataSet.Songs.Add(c.Clone());
                        }
                        foreach (var c in hd.Characters)
                        {
                            _DataSet.Characters.Add(c.Clone());
                        }
                        foreach (var c in hd.Cards)
                        {
                            _DataSet.Cards.Add(c.Clone());
                        }
                    }
                }
            }
            catch
            {
            }
        }
        else
        {
            _OutputDirectory.Create();
            foreach (var category in new[] { "トップス", "ワンピ", "ボトムス", "シューズ", "アクセ" })
            {
                AddCategory(category);
            }
        }
        _CacheDirectory = new DirectoryInfo(Path.Combine(GetDirectory(), "download"));
        _Cache = new();
        var jp = Path.Combine(_CacheDirectory.FullName, "_list.json");
        if (_CacheDirectory.Exists)
        {
            try
            {
                if (File.Exists(jp))
                {
                    _JsonStream = new FileStream(jp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

                    foreach (var e in JsonSerializer.Deserialize<UrlCache[]>(_JsonStream)!)
                    {
                        if (!string.IsNullOrEmpty(e?.Url))
                        {
                            e.LastRequested ??= DateTime.Now.AddDays(-1);
                            _Cache[e.Url] = e;
                            _LastId = Math.Max(e.Id, _LastId);
                        }
                    }
                }
            }
            catch
            {
                _JsonStream = null!;
            }
        }
        else
        {
            _CacheDirectory.Create();
        }
        _JsonStream ??= new FileStream(jp, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    internal AipriDataSet DataSet => _DataSet;

    internal static string GetDirectory([CallerFilePath] string filePath = "")
        => Path.GetDirectoryName(filePath)!;

    public async Task<Stream> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        UrlCache? c;
        string? etag = null;
        DateTime? lastModified = null;
        lock (_Cache)
        {
            if (_Cache.TryGetValue(url, out c)
                && c.Id > 0
                && File.Exists(Path.Combine(_CacheDirectory.FullName, c.Id.ToString())))
            {
                etag = c.Etag;
                lastModified = c.LastModified;

                if (etag != null && c.LastRequested?.AddHours(1) > DateTime.Now)
                {
                    return File.OpenRead(Path.Combine(_CacheDirectory.FullName, c.Id.ToString()));
                }
            }
            else
            {
                _Cache[url] = c = new UrlCache()
                {
                    Url = url
                };
            }
        }

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        if (etag != null)
        {
            req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
        }
        req.Headers.IfModifiedSince = lastModified;

        // Console.WriteLine("Getting {0}.", url);
        var res = await _Http.SendAsync(req).ConfigureAwait(false);

        if (res.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            // Console.WriteLine("Found cache for {0}.", url);
            return File.OpenRead(Path.Combine(_CacheDirectory.FullName, c.Id.ToString()));
        }

        lastModified = res.Content.Headers.LastModified?.UtcDateTime;
        etag = res.Headers.ETag?.Tag;
        if (res.IsSuccessStatusCode)
        {
            Console.WriteLine(
                "GOT {0}. ({1}, {2} bytes, {3}, {4:yyyyMMddHHmmss})",
                url,
                res.Content.Headers.ContentType?.MediaType ?? "no Content-Type",
                res.Content.Headers.ContentLength ?? -1, etag, lastModified);
        }
        else
        {
            Console.Error?.WriteLine(
                "Failed to GET {0}. ({1})",
                url,
                res.StatusCode);

            res.EnsureSuccessStatusCode();
        }

        if (lastModified == null && etag == null)
        {
            return await res.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            int newId;
            lock (_Cache)
            {
                newId = ++_LastId;
            }

            using var ss = await res.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            var fs = new FileStream(Path.Combine(_CacheDirectory.FullName, newId.ToString()), FileMode.Create);

            await ss.CopyToAsync(fs).ConfigureAwait(false);

            fs.Position = 0;

            lock (_Cache)
            {
                _Cache[url] = new UrlCache()
                {
                    Id = newId,
                    Url = url,
                    Etag = etag,
                    LastModified = lastModified,
                    LastRequested = DateTime.Now,
                };
            }

            return fs;
        }
    }

    public Chapter AddVerseChapter(string id, string name)
    {
        lock (_DataSet)
        {
            var c = _DataSet.VerseChapters.GetById(id);
            if (c == null)
            {
                c = new();
                c.Id = id;
                _DataSet.VerseChapters.Add(c);
            }

            c.Name = name;

            return c;
        }
    }

    public Chapter AddHimitsuChapter(string id, string name)
    {
        lock (_DataSet)
        {
            var c = _DataSet.HimitsuChapters.GetById(id);
            if (c == null)
            {
                c = new();
                c.Id = id;
                _DataSet.HimitsuChapters.Add(c);
            }

            c.Name = name;

            return c;
        }
    }

    public async Task<Brand> AddBrandAsync(string name, string? imageUrl)
    {
        Brand? b;
        lock (_DataSet)
        {
            b = _DataSet.Brands.GetByName(name);
            if (b == null)
            {
                b = new()
                {
                    Name = name,
                    Id = (_DataSet.Brands.Max(e => e?.Id) ?? 0) + 1
                };
                _DataSet.Brands.Add(b);
            }
        }

        if (b.ImageUrl == imageUrl && b.IsImageLoaded)
        {
            return b;
        }

        b.ImageUrl = await GetOrCopyImageAsync(imageUrl, "brands", b.Id).ConfigureAwait(false);
        b.IsImageLoaded = true;

        return b;
    }

    public Category? AddCategory(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        Category? c;
        lock (_DataSet)
        {
            c = _DataSet.Categories.GetByName(name);
            if (c == null)
            {
                c = new()
                {
                    Name = name,
                    Id = (_DataSet.Categories.Max(e => e?.Id) ?? 0) + 1
                };
                _DataSet.Categories.Add(c);
            }
        }

        return c;
    }
    public Song? AddSong(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        Song? c;
        lock (_DataSet)
        {
            c = _DataSet.Songs.GetByName(name);
            if (c == null)
            {
                c = new()
                {
                    Name = name,
                    Id = (_DataSet.Songs.Max(e => e?.Id) ?? 0) + 1
                };
                _DataSet.Songs.Add(c);
            }
        }

        return c;
    }
    public Character? AddCharacter(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        Character? c;
        lock (_DataSet)
        {
            c = _DataSet.Characters.GetByName(name);
            if (c == null)
            {
                c = new()
                {
                    Name = name,
                    Id = (_DataSet.Characters.Max(e => e?.Id) ?? 0) + 1
                };
                _DataSet.Characters.Add(c);
            }
        }

        return c;
    }

    public async Task<string?> GetOrCopyImageAsync(string? imageUrl, string directory, int id, string? suffix = null)
    {
        var bip = new FileInfo(Path.Combine(_OutputDirectory.FullName, directory, id.ToString("D6") + suffix + Path.GetExtension(imageUrl)));

        if (!string.IsNullOrEmpty(imageUrl))
        {
            try
            {
                if (!bip.Directory!.Exists)
                {
                    bip.Directory.Create();
                }

                using var ws = await (
                    imageUrl.StartsWith("http") ? GetAsync(imageUrl)
                    : Task.FromResult<Stream>(
                        new FileStream(
                            Path.Combine(GetCustomDirectory(), imageUrl),
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read))).ConfigureAwait(false);

                if (ws is FileStream fs)
                {
                    if (fs.Name != bip.FullName)
                    {
                        File.Copy(fs.Name, bip.FullName, true);
                    }
                }
                else
                {
                    using var fs2 = new FileStream(bip.FullName, FileMode.Create);
                    await ws.CopyToAsync(fs2).ConfigureAwait(false);
                }

                return imageUrl;
            }
            catch { }
        }

        if (bip.Exists)
        {
            bip.Delete();
        }

        return null;
    }

    public async Task<Coordinate> AddCoordinateAsync(Chapter? chapter, Brand? brand, string? kind, string name, int? star, string? imageUrl)
    {
        Coordinate? c;
        lock (_DataSet)
        {
            c = _DataSet.Coordinates.FirstOrDefault(e => e.ChapterId == chapter?.Id && e.Kind == kind && e.Name == name);
            if (c == null)
            {
                c = new()
                {
                    Name = name,
                    ChapterId = chapter?.Id,
                    Kind = kind,
                    Id = (_DataSet.Coordinates.Where(e => e.Id < 900000).Max(e => e?.Id) ?? 0) + 1
                };
                _DataSet.Coordinates.Add(c);
            }
        }

        c.BrandId = brand?.Id;
        c.Star = (byte?)star;

        //if (c.ChapterId != chapter?.Id
        //    && string.IsNullOrEmpty(imageUrl))
        //{
        //    return c;
        //}

        //c.ChapterId = chapter?.Id;

        if (c.ImageUrl == imageUrl && c.IsImageLoaded)
        {
            return c;
        }

        c.ImageUrl = await GetOrCopyImageAsync(imageUrl, "coordinates", c.Id).ConfigureAwait(false);
        c.IsImageLoaded = true;

        return c;
    }

    public async Task<CoordinateItem> AddItemAsync(Coordinate coordinate, string sealId, string term, short point, string? imageUrl, int? estimatedId)
    {
        var category = AddCategory(term)?.Id ?? 0;

        CoordinateItem? c;
        lock (_DataSet)
        {
            c = _DataSet.CoordinateItems.FirstOrDefault(e => e.CoordinateId == coordinate.Id && e.CategoryId == category);
            if (c == null)
            {
                c = new()
                {
                    CoordinateId = coordinate.Id,
                    SealId = sealId,
                    Id = estimatedId > 0 && _DataSet.CoordinateItems.All(e => e.Id != estimatedId) ? estimatedId.Value : ((_DataSet.CoordinateItems.Max(e => e?.Id) ?? 0) + 1)
                };
                _DataSet.CoordinateItems.Add(c);
            }
        }

        c.CoordinateId = coordinate.Id;
        c.CategoryId = category;
        c.Point = point;

        if (!string.IsNullOrEmpty(sealId))
        {
            c.SealId = sealId;
            if (!string.IsNullOrEmpty(c.ImageUrl))
            {
                if (!c.IsImageLoaded)
                {
                    c.ImageUrl = await GetOrCopyImageAsync(c.ImageUrl, "coordinateItems", c.Id).ConfigureAwait(false);
                    c.IsImageLoaded = true;
                }
                return c;
            }
        }

        if (c.ImageUrl == imageUrl && c.IsImageLoaded)
        {
            return c;
        }

        c.ImageUrl = await GetOrCopyImageAsync(imageUrl, "coordinateItems", c.Id).ConfigureAwait(false);
        c.IsImageLoaded = true;

        return c;
    }

    public async Task<Card> AddCardAsync(Chapter? chapter, string coordinate, string character, string sealId, string? image1Url, string? image2Url, string? variant = null)
    {
        variant = variant.TrimOrNull();
        Card? c;
        lock (_DataSet)
        {
            c = _DataSet.Cards.FirstOrDefault(e => e.ChapterId == chapter?.Id && e.Image1Url == image1Url);
            if (c == null)
            {
                c = new()
                {
                    Coordinate = coordinate,
                    Variant = variant,
                    ChapterId = chapter?.Id,
                    Id = (_DataSet.Cards.Where(e => e.Id < 900000).Max(e => e?.Id) ?? 0) + 1
                };
                _DataSet.Cards.Add(c);
            }
        }

        c.SealId = sealId;
        c.Coordinate = coordinate;
        c.CharacterId = AddCharacter(character)?.Id ?? 0;
        c.Variant = variant;

        if (c.Image1Url != image1Url || !c.IsImage1Loaded)
        {
            c.Image1Url = await GetOrCopyImageAsync(image1Url, "cards", c.Id, "-1").ConfigureAwait(false);
            c.IsImage1Loaded = true;
        }

        if (c.Image2Url != image2Url || !c.IsImage2Loaded)
        {
            c.Image2Url = await GetOrCopyImageAsync(image2Url, "cards", c.Id, "-2").ConfigureAwait(false);
            c.IsImage2Loaded = true;
        }

        return c;
    }

    public void Dispose()
    {
        var hpn = Path.Combine(_OutputDirectory.FullName, "data.json.new");
        var hp = Path.Combine(_OutputDirectory.FullName, "data.json");
        try
        {
            using (var fs = new FileStream(hpn, FileMode.Create))
            {
                JsonSerializer.Serialize(fs, new AipriDataSet()
                {
                    VerseChapters = new(_DataSet.VerseChapters.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Categories = new(_DataSet.Categories.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Brands = new(_DataSet.Brands.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Coordinates = new(_DataSet.Coordinates.OrderBy(e => e.Id).Select(e => e.Clone())),
                    CoordinateItems = new(_DataSet.CoordinateItems.OrderBy(e => e.Id).Select(e => e.Clone())),

                    HimitsuChapters = new(_DataSet.HimitsuChapters.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Characters = new(_DataSet.Characters.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Songs = new(_DataSet.Songs.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Cards = new(_DataSet.Cards.OrderBy(e => e.Id).Select(e => e.Clone())),
                }, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
                });
            }

            File.Move(hpn, hp, true);
        }
        catch { }

        try
        {
            _JsonStream.Position = 0;
            _JsonStream.SetLength(0);
            JsonSerializer.Serialize(_JsonStream, _Cache.Values.Where(e => e.Id > 0).OrderBy(e => e.Id).ToArray());
        }
        catch { }
        _JsonStream.Dispose();
        _Http.Dispose();
    }

    public void ClearSubdirectories()
    {
        try
        {
            foreach (var d in _OutputDirectory.GetDirectories())
            {
                d.Delete(true);
            }
        }
        catch { }
    }

    public string GetCustomDirectory() => Path.Combine(Path.GetDirectoryName(_OutputDirectory.FullName)!, "custom");

    public async Task<List<CorrectionEntry<Coordinate>>> EnumerateCoordinateCorrection()
    {
        var list = new List<CorrectionEntry<Coordinate>>();
        var cor = new FileInfo(Path.Combine(GetCustomDirectory(), "_Coordinates.tsv"));
        if (cor.Exists)
        {
            using var fs = cor.OpenRead();
            using var sr = new StreamReader(fs, Encoding.GetEncoding(932));

            var header = await sr.ReadLineAsync().ConfigureAwait(false);

            if (header != null)
            {
                var ha = header.Split('\t');
                var key = Array.IndexOf(ha, "Key");
                var id = Array.IndexOf(ha, nameof(Coordinate.Id));
                var chapterId = Array.IndexOf(ha, nameof(Coordinate.ChapterId));
                var name = Array.IndexOf(ha, nameof(Coordinate.Name));
                var group = Array.IndexOf(ha, nameof(Coordinate.Group));
                var kind = Array.IndexOf(ha, nameof(Coordinate.Kind));
                var star = Array.IndexOf(ha, nameof(Coordinate.Star));
                var brand = Array.IndexOf(ha, "Brand");

                if (key >= 0)
                {
                    var brands = DataSet.Brands.ToDictionary(e => e.Name, e => e.Id);
                    for (var l = await sr.ReadLineAsync().ConfigureAwait(false); l != null; l = await sr.ReadLineAsync().ConfigureAwait(false))
                    {
                        var row = l.Split('\t');

                        string? read(int id)
                            => id >= 0 ? row.ElementAtOrDefault(id).TrimOrNull() : null;
                        list.Add(new()
                        {
                            Key = row.ElementAt(key),
                            Data = new()
                            {
                                Id = int.TryParse(read(id), out var i) ? i : 0,
                                ChapterId = read(chapterId) ?? string.Empty,
                                Name = read(name) ?? string.Empty,
                                Group = read(group) ?? string.Empty,
                                Kind = read(kind) ?? string.Empty,
                                Star = byte.TryParse(read(star), out var st) ? st : null,
                                BrandId = brands.TryGetValue(read(brand) ?? string.Empty, out var bid) ? bid : null
                            }
                        });
                    }
                }
            }
        }

        return list;
    }

    public async Task<List<CorrectionEntry<CoordinateItem>>> EnumerateCoordinateItemCorrection(DownloadContext d)
    {
        var list = new List<CorrectionEntry<CoordinateItem>>();
        var cor = new FileInfo(Path.Combine(GetCustomDirectory(), "_CoordinateItems.tsv"));
        if (cor.Exists)
        {
            using var fs = cor.OpenRead();
            using var sr = new StreamReader(fs, Encoding.GetEncoding(932));

            var header = await sr.ReadLineAsync().ConfigureAwait(false);

            if (header != null)
            {
                var ha = header.Split('\t');
                var key = Array.IndexOf(ha, "Key");
                var id = Array.IndexOf(ha, nameof(CoordinateItem.Id));
                var coordinateId = Array.IndexOf(ha, nameof(CoordinateItem.CoordinateId));
                var sealId = Array.IndexOf(ha, nameof(CoordinateItem.SealId));
                var term = Array.IndexOf(ha, nameof(CoordinateItem.Term));
                var point = Array.IndexOf(ha, nameof(CoordinateItem.Point));
                var brand = Array.IndexOf(ha, "Brand");
                var imageUrl = Array.IndexOf(ha, nameof(CoordinateItem.ImageUrl));

                if (key >= 0)
                {
                    for (var l = await sr.ReadLineAsync().ConfigureAwait(false); l != null; l = await sr.ReadLineAsync().ConfigureAwait(false))
                    {
                        var row = l.Split('\t');

                        string? read(int id)
                            => id >= 0 ? row.ElementAtOrDefault(id).TrimOrNull() : null;
                        list.Add(new()
                        {
                            Key = row.ElementAt(key),
                            Data = new()
                            {
                                Id = int.TryParse(read(id), out var i) ? i : 0,
                                CoordinateId = int.TryParse(read(coordinateId), out var cid) ? cid : default,
                                SealId = read(sealId) ?? string.Empty,
                                CategoryId = d.AddCategory(read(term))?.Id ?? 0,
                                Point = short.TryParse(read(point), out var s) ? s : default,
                                ImageUrl = read(imageUrl) ?? string.Empty,
                            }
                        });
                    }
                }
            }
        }

        return list;
    }

    public async Task<List<CorrectionEntry<Card>>> EnumerateCardCorrection(DownloadContext d)
    {
        var list = new List<CorrectionEntry<Card>>();
        var cor = new FileInfo(Path.Combine(GetCustomDirectory(), "_Cards.tsv"));
        if (cor.Exists)
        {
            using var fs = cor.OpenRead();
            using var sr = new StreamReader(fs, Encoding.GetEncoding(932));

            var header = await sr.ReadLineAsync().ConfigureAwait(false);

            if (header != null)
            {
                var ha = header.Split('\t');
                var key = Array.IndexOf(ha, "Key");
                var id = Array.IndexOf(ha, nameof(Card.Id));
                var chapterId = Array.IndexOf(ha, nameof(Card.ChapterId));
                var sealId = Array.IndexOf(ha, nameof(Card.SealId));
                var coordinate = Array.IndexOf(ha, nameof(Card.Coordinate));
                var character = Array.IndexOf(ha, nameof(Card.Character));
                var variant = Array.IndexOf(ha, nameof(Card.Variant));
                var song = Array.IndexOf(ha, nameof(Card.Song));
                var point = Array.IndexOf(ha, nameof(Card.Point));
                var star = Array.IndexOf(ha, nameof(Card.Star));
                var isChance = Array.IndexOf(ha, nameof(Card.IsChance));
                var brand = Array.IndexOf(ha, "Brand");
                var image1Url = Array.IndexOf(ha, nameof(Card.Image1Url));
                var image2Url = Array.IndexOf(ha, nameof(Card.Image2Url));

                if (key >= 0)
                {
                    var brands = DataSet.Brands.ToDictionary(e => e.Name, e => e.Id);
                    for (var l = await sr.ReadLineAsync().ConfigureAwait(false); l != null; l = await sr.ReadLineAsync().ConfigureAwait(false))
                    {
                        var row = l.Split('\t');

                        string? read(int id)
                            => id >= 0 ? row.ElementAtOrDefault(id).TrimOrNull() : null;
                        list.Add(new()
                        {
                            Key = row.ElementAt(key),
                            Data = new()
                            {
                                Id = int.TryParse(read(id), out var i) ? i : 0,
                                ChapterId = read(chapterId) ?? string.Empty,
                                SealId = read(sealId) ?? string.Empty,
                                Coordinate = read(coordinate) ?? string.Empty,
                                CharacterId = d.AddCharacter(read(character))?.Id ?? 0,
                                Variant = read(variant) ?? string.Empty,
                                SongId = d.AddSong(read(song))?.Id ?? 0,
                                Point = short.TryParse(read(point), out var s) ? s : default,
                                Star = byte.TryParse(read(star), out var st) ? st : (byte)0,
                                IsChance = bool.TryParse(read(isChance), out var b) && b,
                                BrandId = brands.TryGetValue(read(brand) ?? string.Empty, out var bid) ? bid : null,
                                Image1Url = read(image1Url) ?? string.Empty,
                                Image2Url = read(image2Url) ?? string.Empty,
                            }
                        });
                    }
                }
            }
        }

        return list;
    }
}