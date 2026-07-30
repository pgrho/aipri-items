using Shipwreck.ViewModelUtils;

namespace Shipwreck.Aipri.CustomEditor;

public sealed class CardViewModel : ObservableModel
{
    internal CardViewModel(MainWindowViewModel window)
    {
        Window = window;
    }

    public MainWindowViewModel Window { get; }

    #region Key

    private CoordinateKey _CurrentKey;
    private CoordinateKey _NewKey;
    private bool _IsKeyChanged;

    public CoordinateKey CurrentKey
    {
        get => _CurrentKey;
        internal set
        {
            if (SetProperty(ref _CurrentKey, value))
            {
                NewKey = _CurrentKey;
                IsKeyChanged = _NewKey != _CurrentKey;
            }
        }
    }

    public CoordinateKey NewKey
    {
        get => _NewKey;
        set
        {
            if (SetProperty(ref _NewKey, value))
            {
                IsKeyChanged = _NewKey != _CurrentKey;
            }
        }
    }

    public bool IsKeyChanged
    {
        get => _IsKeyChanged;
        internal set => SetProperty(ref _IsKeyChanged, value);
    }

    #endregion Key

    #region Chapter

    private string _CurrentChapter = string.Empty;
    private string _NewChapter = string.Empty;
    private bool _IsChapterChanged;

    public string CurrentChapter
    {
        get => _CurrentChapter;
        internal set
        {
            if (SetProperty(ref _CurrentChapter, value?.Trim() ?? string.Empty))
            {
                NewChapter = _CurrentChapter;
                IsChapterChanged = _NewChapter != _CurrentChapter;
            }
        }
    }

    public string NewChapter
    {
        get => _NewChapter;
        set
        {
            if (SetProperty(ref _NewChapter, value?.Trim() ?? string.Empty))
            {
                IsChapterChanged = _NewChapter != _CurrentChapter;
            }
        }
    }

    public bool IsChapterChanged
    {
        get => _IsChapterChanged;
        internal set => SetProperty(ref _IsChapterChanged, value);
    }

    #endregion Chapter

    #region Id

    private int _CurrentId;
    private int _NewId;
    private bool _IsIdChanged;

    public int CurrentId
    {
        get => _CurrentId;
        internal set
        {
            if (SetProperty(ref _CurrentId, Math.Max(value, 0)))
            {
                NewId = _CurrentId;
                IsIdChanged = _NewId != _CurrentId;
            }
        }
    }

    public int NewId
    {
        get => _NewId;
        set
        {
            if (SetProperty(ref _NewId, Math.Max(value, 0)))
            {
                IsIdChanged = _NewId != _CurrentId;
            }
        }
    }

    public bool IsIdChanged
    {
        get => _IsIdChanged;
        internal set => SetProperty(ref _IsIdChanged, value);
    }

    #endregion Id

    #region Order

    private double _CurrentOrder;
    private double _NewOrder;
    private bool _IsOrderChanged;

    public double CurrentOrder
    {
        get => _CurrentOrder;
        internal set
        {
            if (SetProperty(ref _CurrentOrder, Math.Max(value, 0)))
            {
                NewOrder = _CurrentOrder;
                IsOrderChanged = _NewOrder != _CurrentOrder;
            }
        }
    }

    public double NewOrder
    {
        get => _NewOrder;
        set
        {
            if (SetProperty(ref _NewOrder, Math.Max(value, 0)))
            {
                IsOrderChanged = _NewOrder != _CurrentOrder;
            }
        }
    }

    public bool IsOrderChanged
    {
        get => _IsOrderChanged;
        internal set => SetProperty(ref _IsOrderChanged, value);
    }

    #endregion Order

    #region SealId

    private string _CurrentSealId = string.Empty;
    private string _NewSealId = string.Empty;
    private bool _IsSealIdChanged;

    public string CurrentSealId
    {
        get => _CurrentSealId;
        internal set
        {
            if (SetProperty(ref _CurrentSealId, value?.Trim() ?? string.Empty))
            {
                NewSealId = _CurrentSealId;
                IsSealIdChanged = _NewSealId != _CurrentSealId;
            }
        }
    }

    public string NewSealId
    {
        get => _NewSealId;
        set
        {
            if (SetProperty(ref _NewSealId, value?.Trim() ?? string.Empty))
            {
                IsSealIdChanged = _NewSealId != _CurrentSealId;
            }
        }
    }

    public bool IsSealIdChanged
    {
        get => _IsSealIdChanged;
        internal set => SetProperty(ref _IsSealIdChanged, value);
    }

    #endregion SealId

    #region Coordinate

    private string _CurrentCoordinate = string.Empty;
    private string _NewCoordinate = string.Empty;
    private bool _IsCoordinateChanged;

    public string CurrentCoordinate
    {
        get => _CurrentCoordinate;
        internal set
        {
            if (SetProperty(ref _CurrentCoordinate, value?.Trim() ?? string.Empty))
            {
                NewCoordinate = _CurrentCoordinate;
                IsCoordinateChanged = _NewCoordinate != _CurrentCoordinate;
            }
        }
    }

    public string NewCoordinate
    {
        get => _NewCoordinate;
        set
        {
            if (SetProperty(ref _NewCoordinate, value?.Trim() ?? string.Empty))
            {
                IsCoordinateChanged = _NewCoordinate != _CurrentCoordinate;
            }
        }
    }

    public bool IsCoordinateChanged
    {
        get => _IsCoordinateChanged;
        internal set => SetProperty(ref _IsCoordinateChanged, value);
    }

    #endregion Coordinate

    #region Character

    private string _CurrentCharacter = string.Empty;
    private string _NewCharacter = string.Empty;
    private bool _IsCharacterChanged;

    public string CurrentCharacter
    {
        get => _CurrentCharacter;
        internal set
        {
            if (SetProperty(ref _CurrentCharacter, value?.Trim() ?? string.Empty))
            {
                NewCharacter = _CurrentCharacter;
                IsCharacterChanged = _NewCharacter != _CurrentCharacter;
            }
        }
    }

    public string NewCharacter
    {
        get => _NewCharacter;
        set
        {
            if (SetProperty(ref _NewCharacter, value?.Trim() ?? string.Empty))
            {
                IsCharacterChanged = _NewCharacter != _CurrentCharacter;
            }
        }
    }

    public bool IsCharacterChanged
    {
        get => _IsCharacterChanged;
        internal set => SetProperty(ref _IsCharacterChanged, value);
    }

    #endregion Character

    #region Variant

    private string _CurrentVariant = string.Empty;
    private string _NewVariant = string.Empty;
    private bool _IsVariantChanged;

    public string CurrentVariant
    {
        get => _CurrentVariant;
        internal set
        {
            if (SetProperty(ref _CurrentVariant, value?.Trim() ?? string.Empty))
            {
                NewVariant = _CurrentVariant;
                IsVariantChanged = _NewVariant != _CurrentVariant;
            }
        }
    }

    public string NewVariant
    {
        get => _NewVariant;
        set
        {
            if (SetProperty(ref _NewVariant, value?.Trim() ?? string.Empty))
            {
                IsVariantChanged = _NewVariant != _CurrentVariant;
            }
        }
    }

    public bool IsVariantChanged
    {
        get => _IsVariantChanged;
        internal set => SetProperty(ref _IsVariantChanged, value);
    }

    #endregion Variant

    #region Song

    private string _CurrentSong = string.Empty;
    private string _NewSong = string.Empty;
    private bool _IsSongChanged;

    public string CurrentSong
    {
        get => _CurrentSong;
        internal set
        {
            if (SetProperty(ref _CurrentSong, value?.Trim() ?? string.Empty))
            {
                NewSong = _CurrentSong;
                IsSongChanged = _NewSong != _CurrentSong;
            }
        }
    }

    public string NewSong
    {
        get => _NewSong;
        set
        {
            if (SetProperty(ref _NewSong, value?.Trim() ?? string.Empty))
            {
                if (!string.IsNullOrEmpty(_NewSong) && !Window.Songs.Contains(_NewSong))
                {
                    Window.Songs.Add(_NewSong);
                }
                IsSongChanged = _NewSong != _CurrentSong;
            }
        }
    }

    public bool IsSongChanged
    {
        get => _IsSongChanged;
        internal set => SetProperty(ref _IsSongChanged, value);
    }

    #endregion Song

    #region Point

    private int _CurrentPoint;
    private int _NewPoint;
    private bool _IsPointChanged;

    public int CurrentPoint
    {
        get => _CurrentPoint;
        internal set
        {
            if (SetProperty(ref _CurrentPoint, Math.Max(value, 0)))
            {
                NewPoint = _CurrentPoint;
                IsPointChanged = _NewPoint != _CurrentPoint;
            }
        }
    }

    public int NewPoint
    {
        get => _NewPoint;
        set
        {
            if (SetProperty(ref _NewPoint, Math.Max(value, 0)))
            {
                IsPointChanged = _NewPoint != _CurrentPoint;
            }
        }
    }

    public bool IsPointChanged
    {
        get => _IsPointChanged;
        internal set => SetProperty(ref _IsPointChanged, value);
    }

    #endregion Point

    #region Star

    private int _CurrentStar;
    private int _NewStar;
    private bool _IsStarChanged;

    public int CurrentStar
    {
        get => _CurrentStar;
        internal set
        {
            if (SetProperty(ref _CurrentStar, Math.Max(value, 0)))
            {
                NewStar = _CurrentStar;
                IsStarChanged = _NewStar != _CurrentStar;
            }
        }
    }

    public int NewStar
    {
        get => _NewStar;
        set
        {
            if (SetProperty(ref _NewStar, Math.Max(value, 0)))
            {
                IsStarChanged = _NewStar != _CurrentStar;
            }
        }
    }

    public bool IsStarChanged
    {
        get => _IsStarChanged;
        internal set => SetProperty(ref _IsStarChanged, value);
    }

    #endregion Star

    #region Chance

    private bool _CurrentChance;
    private bool _NewChance;
    private bool _IsChanceChanged;

    public bool CurrentChance
    {
        get => _CurrentChance;
        internal set
        {
            if (SetProperty(ref _CurrentChance, value))
            {
                NewChance = _CurrentChance;
                IsChanceChanged = _NewChance != _CurrentChance;
            }
        }
    }

    public bool NewChance
    {
        get => _NewChance;
        set
        {
            if (SetProperty(ref _NewChance, value))
            {
                IsChanceChanged = _NewChance != _CurrentChance;
            }
        }
    }

    public bool IsChanceChanged
    {
        get => _IsChanceChanged;
        internal set => SetProperty(ref _IsChanceChanged, value);
    }

    #endregion Chance

    #region Brand

    private string _CurrentBrand = string.Empty;
    private string _NewBrand = string.Empty;
    private bool _IsBrandChanged;

    public string CurrentBrand
    {
        get => _CurrentBrand;
        internal set
        {
            if (SetProperty(ref _CurrentBrand, value?.Trim() ?? string.Empty))
            {
                NewBrand = _CurrentBrand;
                IsBrandChanged = _NewBrand != _CurrentBrand;
            }
        }
    }

    public string NewBrand
    {
        get => _NewBrand;
        set
        {
            if (SetProperty(ref _NewBrand, value?.Trim() ?? string.Empty))
            {
                IsBrandChanged = _NewBrand != _CurrentBrand;
            }
        }
    }

    public bool IsBrandChanged
    {
        get => _IsBrandChanged;
        internal set => SetProperty(ref _IsBrandChanged, value);
    }

    #endregion Brand

    #region Image1

    private string _CurrentImage1 = string.Empty;
    private string _NewImage1 = string.Empty;
    private bool _IsImage1Changed;

    public string CurrentImage1
    {
        get => _CurrentImage1;
        internal set
        {
            if (SetProperty(ref _CurrentImage1, value?.Trim() ?? string.Empty))
            {
                NewImage1 = _CurrentImage1;
                IsImage1Changed = _NewImage1 != _CurrentImage1;
            }
        }
    }

    public string NewImage1
    {
        get => _NewImage1;
        set
        {
            if (SetProperty(ref _NewImage1, value?.Trim() ?? string.Empty))
            {
                IsImage1Changed = _NewImage1 != _CurrentImage1;
            }
        }
    }

    public bool IsImage1Changed
    {
        get => _IsImage1Changed;
        internal set => SetProperty(ref _IsImage1Changed, value);
    }

    #endregion Image1

    #region Image2

    private string _CurrentImage2 = string.Empty;
    private string _NewImage2 = string.Empty;
    private bool _IsImage2Changed;

    public string CurrentImage2
    {
        get => _CurrentImage2;
        internal set
        {
            if (SetProperty(ref _CurrentImage2, value?.Trim() ?? string.Empty))
            {
                NewImage2 = _CurrentImage2;
                IsImage2Changed = _NewImage2 != _CurrentImage2;
            }
        }
    }

    public string NewImage2
    {
        get => _NewImage2;
        set
        {
            if (SetProperty(ref _NewImage2, value?.Trim() ?? string.Empty))
            {
                IsImage2Changed = _NewImage2 != _CurrentImage2;
            }
        }
    }

    public bool IsImage2Changed
    {
        get => _IsImage2Changed;
        internal set => SetProperty(ref _IsImage2Changed, value);
    }

    #endregion Image2
}
