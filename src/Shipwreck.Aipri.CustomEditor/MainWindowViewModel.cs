using System.Data;
using System.Runtime.CompilerServices;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.Aipri.CustomEditor;

public sealed class MainWindowViewModel : WindowViewModel
{
    public MainWindowViewModel(MainWindow window)
    {
        Window = window;
    }

    private string GetCustomDirectory([CallerFilePath] string path = "")
        => new Uri(new Uri(path), "../../custom").LocalPath;

    #region プリフォト

    #region Coordinates

    private BulkUpdateableCollection<CoordinateViewModel>? _Coordinates;

    public BulkUpdateableCollection<CoordinateViewModel> Coordinates
    {
        get
        {
            if (_Coordinates == null)
            {
                _Coordinates = new();
                LoadCoordinatesAsync().GetHashCode();
            }
            return _Coordinates;
        }
    }

    private async Task LoadCoordinatesAsync()
    {
        async Task<List<CoordinateViewModel>> load()
        {
            var list = new List<CoordinateViewModel>();

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
        }
        catch { }
    }

    #endregion Coordinates

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
        }, title: "追加", icon: "fas fa-plus");
    #endregion Coordinates

    #region AddNewCoordinateCommand

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

    #endregion AddNewCoordinateCommand

    #endregion プリフォト
}
