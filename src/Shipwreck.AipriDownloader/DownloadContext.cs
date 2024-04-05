﻿using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;

namespace Shipwreck.AipriDownloader;

public sealed class DownloadContext : IDisposable
{
    private readonly HttpClient _Http;
    private readonly AipriVerseDataSet _DataSet;

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
        _OutputDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(GetDirectory())!)!, "output"));
        _DataSet = new();
        if (_OutputDirectory.Exists)
        {
            try
            {
                var cf = new FileInfo(Path.Combine(_OutputDirectory.FullName, "verse.json"));
                if (cf.Exists)
                {
                    using var fs = cf.OpenRead();
                    var hd = JsonSerializer.Deserialize<AipriVerseDataSet>(fs);
                    if (hd != null)
                    {
                        foreach (var c in hd.Chapters ?? [])
                        {
                            if (!string.IsNullOrEmpty(c.Id) && !string.IsNullOrEmpty(c.Name))
                            {
                                _DataSet.Chapters.Add(c.Clone());
                            }
                        }
                        foreach (var c in hd.Brands ?? [])
                        {
                            if (c?.Id > 0
                                && !string.IsNullOrEmpty(c.Name))
                            {
                                _DataSet.Brands.Add(c.Clone());
                            }
                        }
                        foreach (var c in hd.Coordinates ?? [])
                        {
                            if (c?.Id > 0
                                && !string.IsNullOrEmpty(c.Name))
                            {
                                _DataSet.Coordinates.Add(c.Clone());
                            }
                        }
                        foreach (var c in hd.CoordinateItems ?? [])
                        {
                            if (c?.Id > 0
                                && !string.IsNullOrEmpty(c.SealId)
                                && !string.IsNullOrEmpty(c.Term))
                            {
                                _DataSet.CoordinateItems.Add(c.Clone());
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

    internal AipriVerseDataSet DataSet => _DataSet;

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
        lock (_DataSet)
        {
            var c = _DataSet.Chapters.GetById(id);
            if (c == null)
            {
                c = new();
                c.Id = id;
                _DataSet.Chapters.Add(c);
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

    public async Task<string?> GetOrCopyImageAsync(string? imageUrl, string directory, int id)
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

    public async Task<Coordinate> AddCoordinateAsync(Chapter? chapter, Brand? brand, string name, int? star, string? imageUrl)
    {
        Coordinate? c;
        lock (_DataSet)
        {
            c = _DataSet.Coordinates.GetByName(name);
            if (c == null)
            {
                c = new()
                {
                    Name = name,
                    Id = (_DataSet.Coordinates.Max(e => e?.Id) ?? 0) + 1
                };
                _DataSet.Coordinates.Add(c);
            }
        }

        c.ChapterId = chapter?.Id;
        c.BrandId = brand?.Id;
        c.Star = (byte?)star;

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
        lock (_DataSet)
        {
            c = _DataSet.CoordinateItems.FirstOrDefault(e => e.CoordinateId == coordinate.Id && e.SealId == sealId);
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
                JsonSerializer.Serialize(fs, new AipriVerseDataSet()
                {
                    Chapters = new(_DataSet.Chapters.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Brands = new(_DataSet.Brands.OrderBy(e => e.Id).Select(e => e.Clone())),
                    Coordinates = new(_DataSet.Coordinates.OrderBy(e => e.Id).Select(e => e.Clone())),
                    CoordinateItems = new(_DataSet.CoordinateItems.OrderBy(e => e.Id).Select(e => e.Clone())),
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
}