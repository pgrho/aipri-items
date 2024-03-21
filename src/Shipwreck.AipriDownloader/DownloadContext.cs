using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Shipwreck.AipriDownloader;

public sealed class DownloadContext : IDisposable
{
    private readonly HttpClient _Http;
    private readonly Dictionary<string, Chapter> _Chapters;
    private readonly Dictionary<string, Brand> _Brands;
    private readonly Dictionary<string, Coordinate> _Coordinates;
    private readonly Dictionary<string, CoordinateItem> _CoordinateItems;
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
    }

    public DownloadContext()
    {
        _Http = new HttpClient();
        _OutputDirectory = new DirectoryInfo(Path.Combine(GetDirectory(), "output"));
        _Chapters = new();
        _Brands = new();
        _Coordinates = new();
        _CoordinateItems = new();
        if (_OutputDirectory.Exists)
        {
            try
            {
                var cf = new FileInfo(Path.Combine(_OutputDirectory.FullName, "verse.json"));
                if (cf.Exists)
                {
                    using var fs = cf.OpenRead();
                    var hd = JsonSerializer.Deserialize<AipriVerseData>(fs);
                    if (hd != null)
                    {
                        foreach (var c in hd.Chapters ?? [])
                        {
                            if (!string.IsNullOrEmpty(c.Id) && !string.IsNullOrEmpty(c.Name))
                            {
                                _Chapters[c.Id] = c;
                            }
                        }
                        foreach (var c in hd.Brands ?? [])
                        {
                            if (c?.Id > 0
                                && !string.IsNullOrEmpty(c.Name))
                            {
                                _Brands[c.Name] = c;
                            }
                        }
                        foreach (var c in hd.Coordinates ?? [])
                        {
                            if (c?.Id > 0
                                && !string.IsNullOrEmpty(c.Star)
                                && !string.IsNullOrEmpty(c.Name))
                            {
                                _Coordinates[c.Name] = c;
                            }
                        }
                        foreach (var c in hd.CoordinateItems ?? [])
                        {
                            if (c?.Id > 0
                                && !string.IsNullOrEmpty(c.SealId)
                                && !string.IsNullOrEmpty(c.Term))
                            {
                                var coord = _Coordinates.Values.FirstOrDefault(e => e.Id == c.CoordinateId);

                                _CoordinateItems[string.Concat(coord?.Name ?? "?", "/", c.SealId)] = c;
                            }
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

    private static string GetDirectory([CallerFilePath] string filePath = "")
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

        Console.WriteLine("Getting {0}.", url);
        var res = await _Http.SendAsync(req).ConfigureAwait(false);

        if (res.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            Console.WriteLine("Found cache for {0}.", url);
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
                    LastModified = lastModified
                };
            }

            return fs;
        }
    }

    public Chapter AddChapter(string id, string name)
    {
        lock (_Chapters)
        {
            _Chapters.TryGetValue(id, out var c);
            if (c?.Id != id || c?.Name != name)
            {
                c ??= new();
                c.Id = id;
                c.Name = name;
                _Chapters[id] = c;
            }
            return c;
        }
    }

    public async Task<Brand> AddBrandAsync(string name, string? imageUrl)
    {
        Brand? b;
        lock (_Brands)
        {
            if (!_Brands.TryGetValue(name, out b))
            {
                _Brands[name] = b = new Brand()
                {
                    Name = name,
                    Id = (_Brands.Values.Max(e => e?.Id) ?? 0) + 1
                };
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

    private async Task<string?> GetOrCopyImageAsync(string? imageUrl, string directory, int id)
    {
        var bip = new FileInfo(Path.Combine(_OutputDirectory.FullName, directory, id.ToString("D6") + Path.GetExtension(imageUrl)));

        if (!string.IsNullOrEmpty(imageUrl))
        {
            try
            {
                if (!bip.Directory!.Exists)
                {
                    bip.Directory.Create();
                }

                using var ws = await GetAsync(imageUrl).ConfigureAwait(false);

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

    public async Task<Coordinate> AddCoordinateAsync(Chapter? chapter, Brand? brand, string name, string star, string? imageUrl)
    {
        Coordinate? c;
        lock (_Coordinates)
        {
            if (!_Coordinates.TryGetValue(name, out c))
            {
                _Coordinates[name] = c = new()
                {
                    Name = name,
                    Id = (_Coordinates.Values.Max(e => e?.Id) ?? 0) + 1
                };
            }
        }

        c.ChapterId = chapter?.Id;
        c.BrandId = brand?.Id;
        c.Star = star;

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
        CoordinateItem? c;
        var key = string.Concat(coordinate.Name, "/", sealId);
        lock (_CoordinateItems)
        {
            if (!_CoordinateItems.TryGetValue(key, out c))
            {
                _CoordinateItems[key] = c = new()
                {
                    Id = estimatedId > 0 && _CoordinateItems.All(e => e.Value.Id != estimatedId) ? estimatedId.Value : ((_CoordinateItems.Values.Max(e => e?.Id) ?? 0) + 1)
                };
            }
        }

        c.CoordinateId = coordinate.Id;
        c.SealId = sealId;
        c.Term = term;
        c.Point = point;

        if (c.ImageUrl == imageUrl && c.IsImageLoaded)
        {
            return c;
        }

        c.ImageUrl = await GetOrCopyImageAsync(imageUrl, "coordinateItems", c.Id).ConfigureAwait(false);
        c.IsImageLoaded = true;

        return c;
    }

    public void Dispose()
    {
        var hpn = Path.Combine(_OutputDirectory.FullName, "verse.json.new");
        var hp = Path.Combine(_OutputDirectory.FullName, "verse.json");
        try
        {
            using (var fs = new FileStream(hpn, FileMode.Create))
            {
                JsonSerializer.Serialize(fs, new AipriVerseData()
                {
                    Chapters = _Chapters.Values.OrderBy(e => e.Id).ToArray(),
                    Brands = _Brands.Values.OrderBy(e => e.Id).ToArray(),
                    Coordinates = _Coordinates.Values.OrderBy(e => e.Id).ToArray(),
                    CoordinateItems = _CoordinateItems.Values.OrderBy(e => e.Id).ToArray(),
                }, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
}