namespace Shipwreck.Aipri;

public class AipriDataSet
{
    #region VerseChapters

    private ChapterCollection? _VerseChapters;

    public ChapterCollection VerseChapters
    {
        get => _VerseChapters ??= new(this);
        set => VerseChapters.Set(value);
    }

    #endregion VerseChapters

    #region Categories

    private CategoryCollection? _Categories;

    public CategoryCollection Categories
    {
        get => _Categories ??= new(this);
        set => Categories.Set(value);
    }

    #endregion Categories

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

    #region PartCategories

    private CategoryCollection? _PartCategories;

    public CategoryCollection PartCategories
    {
        get => _PartCategories ??= new(this);
        set => PartCategories.Set(value);
    }

    #endregion PartCategories

    #region Parts

    private PartCollection? _Parts;

    public PartCollection Parts
    {
        get => _Parts ??= new(this);
        set => Parts.Set(value);
    }

    #endregion Parts

    #region HimitsuChapters

    private ChapterCollection? _HimitsuChapters;

    public ChapterCollection HimitsuChapters
    {
        get => _HimitsuChapters ??= new(this);
        set => HimitsuChapters.Set(value);
    }

    #endregion HimitsuChapters

    #region Characters

    private CharacterCollection? _Characters;

    public CharacterCollection Characters
    {
        get => _Characters ??= new(this);
        set => Characters.Set(value);
    }

    #endregion Characters

    #region Songs

    private SongCollection? _Songs;

    public SongCollection Songs
    {
        get => _Songs ??= new(this);
        set => Songs.Set(value);
    }

    #endregion Songs

    #region Cards

    private CardCollection? _Cards;

    public CardCollection Cards
    {
        get => _Cards ??= new(this);
        set => Cards.Set(value);
    }

    #endregion Cards
}