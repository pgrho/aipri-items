namespace Shipwreck.Aipri;

public class AipriVerseDataSet
{
    #region VerseChapters

    private ChapterCollection? _VerseChapters;

    public ChapterCollection VerseChapters
    {
        get => _VerseChapters ??= new(this);
        set => VerseChapters.Set(value);
    }

    #endregion VerseChapters

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

    #region HimitsuChapters

    private ChapterCollection? _HimitsuChapters;

    public ChapterCollection HimitsuChapters
    {
        get => _HimitsuChapters ??= new(this);
        set => HimitsuChapters.Set(value);
    }

    #endregion HimitsuChapters

    #region Cards

    private CardCollection? _Cards;

    public CardCollection Cards
    {
        get => _Cards ??= new(this);
        set => Cards.Set(value);
    }

    #endregion Cards
}