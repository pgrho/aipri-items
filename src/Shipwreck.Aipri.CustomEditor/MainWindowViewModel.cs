using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Shipwreck.ViewModelUtils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Shipwreck.Aipri.CustomEditor;

public sealed class MainWindowViewModel : WindowViewModel
{
    public MainWindowViewModel(MainWindow window)
    {
        Window = window;
    }

    private string GetCustomDirectory([CallerFilePath] string path = "", string relativePath = "../../custom")
        => new Uri(new Uri(path), relativePath).LocalPath;

    #region マスター

    private Task<AipriDataSet>? _DataSetTask;

    private Task<AipriDataSet> GetDataSetAsync()
    {
        if (_DataSetTask?.Status switch
        {
            null => true,
            TaskStatus.Faulted => true,
            TaskStatus.Canceled => true,
            _ => false
        })
        {
            async Task<AipriDataSet> load()
            {
                try
                {
                    var fi = new FileInfo(GetCustomDirectory(relativePath: "../../output/data.json"));

                    if (fi.Exists)
                    {
                        using var fs = fi.OpenRead();
                        return JsonSerializer.Deserialize<AipriDataSet>(fs)!;
                    }
                }
                catch
                {
                }
                return new();
            }
            _DataSetTask = load();
        }
        return _DataSetTask!;
    }

    #region VerseChapters

    private BulkUpdateableCollection<Chapter>? _VerseChapters;

    public BulkUpdateableCollection<Chapter> VerseChapters
    {
        get
        {
            if (_VerseChapters == null)
            {
                _VerseChapters = new();
                async void load()
                {
                    try
                    {
                        var t = await GetDataSetAsync();
                        _VerseChapters.SetIfNeeded(t.VerseChapters.Prepend(new() { Id = string.Empty }).ToList());
                    }
                    catch { }
                }
                load();
            }

            return _VerseChapters;
        }
    }

    #endregion VerseChapters

    #region HimitsuChapters

    private BulkUpdateableCollection<Chapter>? _HimitsuChapters;

    public BulkUpdateableCollection<Chapter> HimitsuChapters
    {
        get
        {
            if (_HimitsuChapters == null)
            {
                _HimitsuChapters = new();
                async void load()
                {
                    try
                    {
                        var t = await GetDataSetAsync();
                        _HimitsuChapters.SetIfNeeded(t.HimitsuChapters.Prepend(new() { Id = string.Empty }).ToList());
                    }
                    catch { }
                }
                load();
            }

            return _HimitsuChapters;
        }
    }

    #endregion HimitsuChapters

    #region Songs

    private BulkUpdateableCollection<string>? _Songs;

    public BulkUpdateableCollection<string> Songs
    {
        get
        {
            if (_Songs == null)
            {
                _Songs = new();
                async void load()
                {
                    try
                    {
                        var t = await GetDataSetAsync();
                        _Songs.SetIfNeeded(t.Songs.Select(e => e.Name ?? string.Empty).Prepend(string.Empty).ToList());
                    }
                    catch { }
                }
                load();
            }

            return _Songs;
        }
    }

    #endregion Songs

    #endregion マスター

    #region プリフォト

    #region Coordinates

    private BulkUpdateableCollection<CoordinateViewModel>? _Coordinates;
    private BulkUpdateableCollection<CoordinateViewModel>? _FilteredCoordinates;

    public BulkUpdateableCollection<CoordinateViewModel> Coordinates
    {
        get
        {
            if (_Coordinates == null)
            {
                _Coordinates = new();
                _FilteredCoordinates = new();
                LoadCoordinatesAsync().GetHashCode();
            }
            return _Coordinates;
        }
    }

    public BulkUpdateableCollection<CoordinateViewModel> FilteredCoordinates
    {
        get
        {
            if (_FilteredCoordinates == null)
            {
                Coordinates.GetHashCode();
            }
            return _FilteredCoordinates!;
        }
    }

    private async Task LoadCoordinatesAsync()
    {
        async Task<List<CoordinateViewModel>> load()
        {
            var list = new List<CoordinateViewModel>();

            var ds = await GetDataSetAsync();

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
                    var id = Array.IndexOf(ha, "Id");
                    var chapterId = Array.IndexOf(ha, "ChapterId");
                    var name = Array.IndexOf(ha, "Name");
                    var group = Array.IndexOf(ha, "Group");
                    var kind = Array.IndexOf(ha, "Kind");
                    var star = Array.IndexOf(ha, "Star");
                    var brand = Array.IndexOf(ha, "Brand");
                    var start = Array.IndexOf(ha, "Start");
                    var end = Array.IndexOf(ha, "End");
                    var img = Array.IndexOf(ha, "Image");

                    var cat1 = Array.IndexOf(ha, "Item1Category");
                    var key1 = Array.IndexOf(ha, "Item1Key");
                    var img1 = Array.IndexOf(ha, "Item1Image");
                    var sid1 = Array.IndexOf(ha, "Item1SealId");
                    var pnt1 = Array.IndexOf(ha, "Item1Point");
                    var set1 = Array.IndexOf(ha, "Item1IsSet");
                    var cat2 = Array.IndexOf(ha, "Item2Category");
                    var key2 = Array.IndexOf(ha, "Item2Key");
                    var img2 = Array.IndexOf(ha, "Item2Image");
                    var sid2 = Array.IndexOf(ha, "Item2SealId");
                    var pnt2 = Array.IndexOf(ha, "Item2Point");
                    var set2 = Array.IndexOf(ha, "Item2IsSet");
                    var cat3 = Array.IndexOf(ha, "Item3Category");
                    var key3 = Array.IndexOf(ha, "Item3Key");
                    var img3 = Array.IndexOf(ha, "Item3Image");
                    var sid3 = Array.IndexOf(ha, "Item3SealId");
                    var pnt3 = Array.IndexOf(ha, "Item3Point");
                    var set3 = Array.IndexOf(ha, "Item3IsSet");
                    var cat4 = Array.IndexOf(ha, "Item4Category");
                    var key4 = Array.IndexOf(ha, "Item4Key");
                    var img4 = Array.IndexOf(ha, "Item4Image");
                    var sid4 = Array.IndexOf(ha, "Item4SealId");
                    var pnt4 = Array.IndexOf(ha, "Item4Point");
                    var set4 = Array.IndexOf(ha, "Item4IsSet");

                    if (key >= 0)
                    {
                        for (var l = await sr.ReadLineAsync().ConfigureAwait(false); l != null; l = await sr.ReadLineAsync().ConfigureAwait(false))
                        {
                            var row = l.Split('\t');

                            string? read(int id) => id >= 0 ? row.ElementAtOrDefault(id)?.Trim() : null;

                            var data = new CoordinateViewModel()
                            {
                                CurrentKey = Enum.TryParse(read(key), out CoordinateKey k) ? k : CoordinateKey.Id,
                                CurrentId = int.TryParse(read(id), out var i) ? i : 0,
                                CurrentChapter = read(chapterId) ?? string.Empty,
                                CurrentName = read(name) ?? string.Empty,
                                CurrentGroup = read(group) ?? string.Empty,
                                CurrentKind = read(kind) ?? string.Empty,
                                CurrentStar = byte.TryParse(read(star), out var st) ? st : 0,
                                CurrentBrand = read(brand) ?? string.Empty,
                                CurrentStart = DateTime.TryParse(read(start), out var d1) ? d1 : null,
                                CurrentEnd = DateTime.TryParse(read(end), out var d2) ? d2 : null,
                                CurrentImage = read(img) ?? string.Empty,
                            };

                            void addItem(int categoryColumn, int keyColumn, int imageColumn, int sealIdColumn, int pointColumn, int isSetColumn)
                            {
                                var cat = read(categoryColumn);
                                if (!string.IsNullOrEmpty(cat)
                                    && ItemCategoryExtensions.TryParse(cat, out var cv)
                                    && int.TryParse(read(keyColumn), out var ik) && ik > 0)
                                {
                                    var imgUrl = read(imageColumn);
                                    var point = short.TryParse(read(pointColumn), out var pt) ? pt : default;
                                    var sealId = read(sealIdColumn);
                                    var isSet = bool.TryParse(read(isSetColumn)?.ToLowerInvariant(), out var bv) && bv;

                                    data.AddItem(new()
                                    {
                                        CurrentCategory = cv,
                                        CurrentId = ik,
                                        CurrentPoint = point,
                                        CurrentImage = imgUrl ?? string.Empty,
                                        CurrentSealId = sealId ?? string.Empty,
                                        CurrentSet = isSet,
                                    });
                                }
                            }

                            addItem(cat1, key1, img1, sid1, pnt1, set1);
                            addItem(cat2, key2, img2, sid2, pnt2, set2);
                            addItem(cat3, key3, img3, sid3, pnt3, set3);
                            addItem(cat4, key4, img4, sid4, pnt4, set4);

                            list.Add(data);
                        }
                    }
                }
            }
            return list;
        }

        try
        {
            var items = await load();
            Coordinates.Clear();

            foreach (var e in items)
            {
                Coordinates.Add(e);
            }
            InvalidateCoordinateFilter();
        }
        catch { }
    }

    #endregion Coordinates

    #region CoordinateFilter

    private string _CoordinateFilter = string.Empty;

    public string CoordinateFilter
    {
        get => _CoordinateFilter;
        set
        {
            if (SetProperty(ref _CoordinateFilter, value?.Trim() ?? string.Empty))
            {
                InvalidateCoordinateFilter();
            }
        }
    }

    private void InvalidateCoordinateFilter()
    {
        var f = _CoordinateFilter;
        if (string.IsNullOrEmpty(f))
        {
            FilteredCoordinates.SetIfNeeded(Coordinates);
        }
        else
        {
            FilteredCoordinates.SetIfNeeded(
                Coordinates.Where(e => e.NewKind?.Contains(f, StringComparison.CurrentCultureIgnoreCase) == true || e.NewName?.Contains(f, StringComparison.CurrentCultureIgnoreCase) == true)
                .ToList());
        }
    }

    #endregion CoordinateFilter

    #region AddNewCoordinateCommand

    private CommandViewModelBase? _AddNewCoordinateCommand;

    public CommandViewModelBase AddNewCoordinateCommand
        => _AddNewCoordinateCommand
        ??= CommandViewModel.Create(_ =>
        {
            var row = new CoordinateViewModel();
            row.NewId = (Coordinates.Max(e => e?.NewId) ?? 0) + 1;

            var cid = (Coordinates.SelectMany(e => e.Items).Max(e => e?.NewId) ?? 0);

            row.AddItem(new()
            {
                NewCategory = ItemCategory.Tops,
                NewId = cid + 1
            });
            row.AddItem(new()
            {
                NewCategory = ItemCategory.Bottoms,
                NewId = cid + 2,
                NewSet = true,
            });
            row.AddItem(new()
            {
                NewCategory = ItemCategory.Shoes,
                NewId = cid + 3
            });
            row.AddItem(new()
            {
                NewCategory = ItemCategory.Accessory,
                NewId = cid + 4
            });

            Coordinates.Add(row);
            InvalidateCoordinateFilter();
        }, title: "追加", icon: "fas fa-plus");

    #endregion AddNewCoordinateCommand

    #region SaveCoordinatesCommand

    private CommandViewModelBase? _SaveCoordinatesCommand;

    public CommandViewModelBase SaveCoordinatesCommand
        => _SaveCoordinatesCommand ??= CommandViewModel.CreateAsync(
            async _ =>
            {
                try
                {
                    var src = Coordinates.OrderBy(e => e.NewKey).ThenBy(e => e.NewId).ThenBy(e => e.NewName).ToList();

                    const char TAB = '\t';
                    using (var fs = new FileStream(Path.Combine(GetCustomDirectory(), "_Coordinates.tsv"), FileMode.Create))
                    using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                    {
                        sw.Write("Key"); sw.Write(TAB);
                        sw.Write("ChapterId"); sw.Write(TAB);
                        sw.Write("Id"); sw.Write(TAB);
                        sw.Write("Star"); sw.Write(TAB);
                        sw.Write("Name"); sw.Write(TAB);
                        sw.Write("Group"); sw.Write(TAB);
                        sw.Write("Kind"); sw.Write(TAB);
                        sw.Write("Brand"); sw.Write(TAB);
                        sw.Write("Start"); sw.Write(TAB);
                        sw.Write("End"); sw.Write(TAB);
                        sw.Write("Image"); sw.Write(TAB);
                        sw.Write("Item1Category"); sw.Write(TAB);
                        sw.Write("Item1Key"); sw.Write(TAB);
                        sw.Write("Item1Image"); sw.Write(TAB);
                        sw.Write("Item1SealId"); sw.Write(TAB);
                        sw.Write("Item1Point"); sw.Write(TAB);
                        sw.Write("Item1IsSet"); sw.Write(TAB);
                        sw.Write("Item2Category"); sw.Write(TAB);
                        sw.Write("Item2Key"); sw.Write(TAB);
                        sw.Write("Item2Image"); sw.Write(TAB);
                        sw.Write("Item2SealId"); sw.Write(TAB);
                        sw.Write("Item2Point"); sw.Write(TAB);
                        sw.Write("Item2IsSet"); sw.Write(TAB);
                        sw.Write("Item3Category"); sw.Write(TAB);
                        sw.Write("Item3Key"); sw.Write(TAB);
                        sw.Write("Item3Image"); sw.Write(TAB);
                        sw.Write("Item3SealId"); sw.Write(TAB);
                        sw.Write("Item3Point"); sw.Write(TAB);
                        sw.Write("Item3IsSet"); sw.Write(TAB);
                        sw.Write("Item4Category"); sw.Write(TAB);
                        sw.Write("Item4Key"); sw.Write(TAB);
                        sw.Write("Item4Image"); sw.Write(TAB);
                        sw.Write("Item4SealId"); sw.Write(TAB);
                        sw.Write("Item4Point"); sw.Write(TAB);
                        sw.Write("Item4IsSet");
                        sw.WriteLine();

                        foreach (var s in src)
                        {
                            sw.Write(s.NewKey switch
                            {
                                CoordinateKey.Id => "",
                                _ => s.NewKey.ToString("G")
                            });
                            sw.Write(TAB);
                            sw.Write(s.NewChapter);
                            sw.Write(TAB);
                            sw.Write(s.NewId.PositiveOrNull());
                            sw.Write(TAB);
                            sw.Write(s.NewStar.PositiveOrNull());
                            sw.Write(TAB);
                            sw.Write(s.NewName);
                            sw.Write(TAB);
                            sw.Write(s.NewGroup);
                            sw.Write(TAB);
                            sw.Write(s.NewKind);
                            sw.Write(TAB);
                            sw.Write(s.NewBrand);
                            sw.Write(TAB);
                            sw.Write(s.NewStart?.ToString("yyyy/M/d"));
                            sw.Write(TAB);
                            sw.Write(s.NewEnd?.ToString("yyyy/M/d"));
                            sw.Write(TAB);
                            sw.Write(s.NewImage);

                            foreach (var c in s.Items)
                            {
                                if (c?.NewId > 0)
                                {
                                    sw.Write(TAB);
                                    sw.Write(c.NewCategory.GetDisplayName());
                                    sw.Write(TAB);
                                    sw.Write(c.NewId);
                                    sw.Write(TAB);
                                    sw.Write(c.NewImage);
                                    sw.Write(TAB);
                                    sw.Write(c.NewSealId);
                                    sw.Write(TAB);
                                    sw.Write(c.NewPoint.PositiveOrNull());
                                    sw.Write(TAB);
                                    sw.Write(c.NewSet ? "TRUE" : null);
                                }
                            }

                            sw.WriteLine();
                        }
                    }

                    await LoadCoordinatesAsync();
                }
                catch { }
            },
            title: "保存",
            style: BorderStyle.Primary,
            iconGetter: c => c.IsExecuting ? "fas fa-pulse fa-spinner" : "fas fa-save");

    #endregion SaveCoordinatesCommand

    #endregion プリフォト

    #region カード

    #region Cards

    private BulkUpdateableCollection<CardViewModel>? _Cards;
    private BulkUpdateableCollection<CardViewModel>? _FilteredCards;

    public BulkUpdateableCollection<CardViewModel> Cards
    {
        get
        {
            if (_Cards == null)
            {
                _Cards = new();
                _FilteredCards = new();
                LoadCardsAsync().GetHashCode();
            }
            return _Cards;
        }
    }

    public BulkUpdateableCollection<CardViewModel> FilteredCards
    {
        get
        {
            if (_FilteredCards == null)
            {
                Cards.GetHashCode();
            }
            return _FilteredCards!;
        }
    }

    private async Task LoadCardsAsync()
    {
        async Task<List<CardViewModel>> load()
        {
            var list = new List<CardViewModel>();

            var cor = new FileInfo(Path.Combine(GetCustomDirectory(), "_Cards.tsv"));
            if (cor.Exists)
            {
                using var fs = cor.OpenRead();
                using var sr = new StreamReader(fs, Encoding.UTF8);

                var header = await sr.ReadLineAsync().ConfigureAwait(false);

                if (header != null)
                {
                    var ha = header.Split('\t');
                    var key = Array.IndexOf(ha, "Key");
                    var id = Array.IndexOf(ha, "Id");
                    var chapterId = Array.IndexOf(ha, "ChapterId");
                    var order = Array.IndexOf(ha, "Order");
                    var sealId = Array.IndexOf(ha, "SealId");
                    var coordinate = Array.IndexOf(ha, "Coordinate");
                    var character = Array.IndexOf(ha, "Character");
                    var variant = Array.IndexOf(ha, "Variant");
                    var song = Array.IndexOf(ha, "Song");
                    var point = Array.IndexOf(ha, "Point");
                    var star = Array.IndexOf(ha, "Star");
                    var isChance = Array.IndexOf(ha, "IsChance");
                    var brand = Array.IndexOf(ha, "Brand");
                    var image1Url = Array.IndexOf(ha, "Image1Url");
                    var image2Url = Array.IndexOf(ha, "Image2Url");

                    if (key >= 0)
                    {
                        for (var l = await sr.ReadLineAsync().ConfigureAwait(false); l != null; l = await sr.ReadLineAsync().ConfigureAwait(false))
                        {
                            var row = l.Split('\t');

                            string? read(int id) => id >= 0 ? row.ElementAtOrDefault(id)?.Trim() : null;

                            var data = new CardViewModel(this)
                            {
                                CurrentKey = Enum.TryParse(read(key), out CoordinateKey k) ? k : CoordinateKey.Id,
                                CurrentId = int.TryParse(read(id), out var i) ? i : 0,
                                CurrentChapter = read(chapterId) ?? string.Empty,
                                CurrentSealId = read(sealId) ?? string.Empty,
                                CurrentOrder = double.TryParse(read(order), out var o) ? o : 0,
                                CurrentCoordinate = read(coordinate) ?? string.Empty,
                                CurrentCharacter = read(character) ?? string.Empty,
                                CurrentVariant = read(variant) ?? string.Empty,
                                CurrentSong = read(song) ?? string.Empty,
                                CurrentPoint = int.TryParse(read(point), out var pt) ? pt : 0,
                                CurrentStar = byte.TryParse(read(star), out var st) ? st : 0,
                                CurrentChance = bool.TryParse(read(isChance), out var ic) && ic,
                                CurrentBrand = read(brand) ?? string.Empty,
                                CurrentImage1 = read(image1Url) ?? string.Empty,
                                CurrentImage2 = read(image2Url) ?? string.Empty,
                            };

                            list.Add(data);
                        }
                    }
                }
            }

            var ds = await GetDataSetAsync();
            var bs = ds.Brands.ToDictionary(e => e.Id);

            foreach (var c in ds.Cards)
            {
                if (string.IsNullOrEmpty(c.SealId))
                {
                    continue;
                }

                var t = list.FirstOrDefault(e => e.NewKey switch
                {
                    CoordinateKey.Id => e.NewId == c.Id,
                    CoordinateKey.SealId => e.NewSealId == c.SealId,
                    _ => false
                });

                if (t == null)
                {
                    var data = new CardViewModel(this)
                    {
                        NewKey = CoordinateKey.SealId,
                        NewId = c.Id,
                        NewChapter = c.ChapterId ?? string.Empty,
                        NewSealId = c.SealId,
                        NewOrder = c.Order < int.MaxValue ? c.Order : 0,
                        NewCoordinate = c.Coordinate,
                        NewCharacter = c.Character ?? string.Empty,
                        NewVariant = c.Variant ?? string.Empty,
                        NewSong = c.Song ?? string.Empty,
                        NewPoint = c.Point,
                        NewStar = c.Star,
                        NewChance = c.IsChance,
                        NewBrand = bs.TryGetValue(c.BrandId ?? 0, out var b) ? b.Name : c.BrandId?.ToString() ?? string.Empty,
                        NewImage1 = c.Image1Url ?? string.Empty,
                        NewImage2 = c.Image2Url ?? string.Empty,
                    };

                    list.Add(data);
                }
                else
                {
                    t.NewSong = c.Song.TrimOrNull() ?? t.NewSong;
                }
            }

            list = list.OrderBy(e => e.NewKey).ThenBy(e => e.NewId).ThenBy(e => e.NewSealId).ThenBy(e => e.NewOrder).ToList();

            return list;
        }

        try
        {
            var items = await load();
            Cards.Clear();

            foreach (var e in items)
            {
                Cards.Add(e);
            }
            InvalidateCardFilter();
        }
        catch { }
    }

    #endregion Cards

    #region CardFilter

    private string _CardFilter = string.Empty;

    public string CardFilter
    {
        get => _CardFilter;
        set
        {
            if (SetProperty(ref _CardFilter, value?.Trim() ?? string.Empty))
            {
                InvalidateCardFilter();
            }
        }
    }

    private void InvalidateCardFilter()
    {
        var f = _CardFilter;
        if (string.IsNullOrEmpty(f))
        {
            FilteredCards.SetIfNeeded(Cards);
        }
        else
        {
            FilteredCards.SetIfNeeded(
                Cards.Where(e => e.NewSealId?.Contains(f, StringComparison.CurrentCultureIgnoreCase) == true || e.NewCoordinate?.Contains(f, StringComparison.CurrentCultureIgnoreCase) == true)
                .ToList());
        }
    }

    #endregion CardFilter

    #region AddNewCardCommand

    private CommandViewModelBase? _AddNewCardCommand;

    public CommandViewModelBase AddNewCardCommand
        => _AddNewCardCommand
        ??= CommandViewModel.Create(_ =>
        {
            var row = new CardViewModel(this);
            row.NewId = (Cards.Max(e => e?.NewId) ?? 0) + 1;

            Cards.Add(row);
            InvalidateCardFilter();
        }, title: "追加", icon: "fas fa-plus");

    #endregion AddNewCardCommand

    #region SaveCardsCommand

    private CommandViewModelBase? _SaveCardsCommand;

    public CommandViewModelBase SaveCardsCommand
        => _SaveCardsCommand ??= CommandViewModel.CreateAsync(
            async _ =>
            {
                try
                {
                    var src = Cards.OrderBy(e => e.NewKey).ThenBy(e => e.NewId).ThenBy(e => e.NewSealId).ThenBy(e => e.NewOrder).ToList();

                    const char TAB = '\t';
                    using (var fs = new FileStream(Path.Combine(GetCustomDirectory(), "_Cards.tsv"), FileMode.Create))
                    using (var sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.Write("Key"); sw.Write(TAB);
                        sw.Write("Id"); sw.Write(TAB);
                        sw.Write("ChapterId"); sw.Write(TAB);
                        sw.Write("Order"); sw.Write(TAB);
                        sw.Write("SealId"); sw.Write(TAB);
                        sw.Write("Coordinate"); sw.Write(TAB);
                        sw.Write("Character"); sw.Write(TAB);
                        sw.Write("Variant"); sw.Write(TAB);
                        sw.Write("Song"); sw.Write(TAB);
                        sw.Write("Star"); sw.Write(TAB);
                        sw.Write("Point"); sw.Write(TAB);
                        sw.Write("IsChance"); sw.Write(TAB);
                        sw.Write("Brand"); sw.Write(TAB);
                        sw.Write("Image1Url"); sw.Write(TAB);
                        sw.Write("Image2Url");

                        sw.WriteLine();

                        foreach (var s in src)
                        {
                            sw.Write(s.NewKey switch
                            {
                                CoordinateKey.Id => "",
                                _ => s.NewKey.ToString("G")
                            });
                            sw.Write(TAB);

                            sw.Write(s.NewId.PositiveOrNull());
                            sw.Write(TAB);
                            sw.Write(s.NewChapter);
                            sw.Write(TAB);

                            sw.Write(s.NewOrder.PositiveOrNull());
                            sw.Write(TAB);

                            sw.Write(s.NewSealId); sw.Write(TAB);
                            sw.Write(s.NewCoordinate); sw.Write(TAB);
                            sw.Write(s.NewCharacter); sw.Write(TAB);
                            sw.Write(s.NewVariant); sw.Write(TAB);
                            sw.Write(s.NewSong); sw.Write(TAB);
                            sw.Write(s.NewStar.PositiveOrNull()); sw.Write(TAB);
                            sw.Write(s.NewPoint.PositiveOrNull()); sw.Write(TAB);
                            sw.Write(s.NewChance ? "TRUE" : null); sw.Write(TAB);
                            sw.Write(s.NewBrand); sw.Write(TAB);
                            sw.Write(s.NewImage1); sw.Write(TAB);
                            sw.Write(s.NewImage2);
                            sw.WriteLine();
                        }
                    }

                    await LoadCardsAsync();
                }
                catch { }
            },
            title: "保存",
            style: BorderStyle.Primary,
            iconGetter: c => c.IsExecuting ? "fas fa-pulse fa-spinner" : "fas fa-save");

    #endregion SaveCardsCommand

    #endregion カード
}
