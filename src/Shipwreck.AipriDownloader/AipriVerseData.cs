namespace Shipwreck.AipriDownloader;

public sealed class AipriVerseData
{
    #region Chapters

    private ChapterCollection? _Chapters;

    public ChapterCollection Chapters
    {
        get => _Chapters ??= new(this);
        set => Chapters.Set(value);
    }

    #endregion Chapters

    #region Brands

    private BrandCollection? _Brands;

    public BrandCollection Brands
    {
        get => _Brands ??= new(this);
        set => Brands.Set(value);
    }

    #endregion Brands

    #region Coordinates

    private CoordinateCollection? _Coordinates;

    public CoordinateCollection Coordinates
    {
        get => _Coordinates ??= new(this);
        set => Coordinates.Set(value);
    }

    #endregion Coordinates

    #region CoordinateItems

    private CoordinateItemCollection? _CoordinateItems;

    public CoordinateItemCollection CoordinateItems
    {
        get => _CoordinateItems ??= new(this);
        set => CoordinateItems.Set(value);
    }

    #endregion CoordinateItems
}