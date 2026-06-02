using System.Collections.ObjectModel;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.Aipri.CustomEditor;

public sealed class CoordinateViewModel : ObservableModel
{
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

    #region Name

    private string _CurrentName = string.Empty;
    private string _NewName = string.Empty;
    private bool _IsNameChanged;

    public string CurrentName
    {
        get => _CurrentName;
        internal set
        {
            if (SetProperty(ref _CurrentName, value?.Trim() ?? string.Empty))
            {
                NewName = _CurrentName;
                IsNameChanged = _NewName != _CurrentName;
            }
        }
    }

    public string NewName
    {
        get => _NewName;
        set
        {
            if (SetProperty(ref _NewName, value?.Trim() ?? string.Empty))
            {
                IsNameChanged = _NewName != _CurrentName;
            }
        }
    }

    public bool IsNameChanged
    {
        get => _IsNameChanged;
        internal set => SetProperty(ref _IsNameChanged, value);
    }

    #endregion Name

    #region Group

    private string _CurrentGroup = string.Empty;
    private string _NewGroup = string.Empty;
    private bool _IsGroupChanged;

    public string CurrentGroup
    {
        get => _CurrentGroup;
        internal set
        {
            if (SetProperty(ref _CurrentGroup, value?.Trim() ?? string.Empty))
            {
                NewGroup = _CurrentGroup;
                IsGroupChanged = _NewGroup != _CurrentGroup;
            }
        }
    }

    public string NewGroup
    {
        get => _NewGroup;
        set
        {
            if (SetProperty(ref _NewGroup, value?.Trim() ?? string.Empty))
            {
                IsGroupChanged = _NewGroup != _CurrentGroup;
            }
        }
    }

    public bool IsGroupChanged
    {
        get => _IsGroupChanged;
        internal set => SetProperty(ref _IsGroupChanged, value);
    }

    #endregion Group

    #region Kind

    private string _CurrentKind = string.Empty;
    private string _NewKind = string.Empty;
    private bool _IsKindChanged;

    public string CurrentKind
    {
        get => _CurrentKind;
        internal set
        {
            if (SetProperty(ref _CurrentKind, value?.Trim() ?? string.Empty))
            {
                NewKind = _CurrentKind;
                IsKindChanged = _NewKind != _CurrentKind;
            }
        }
    }

    public string NewKind
    {
        get => _NewKind;
        set
        {
            if (SetProperty(ref _NewKind, value?.Trim() ?? string.Empty))
            {
                IsKindChanged = _NewKind != _CurrentKind;
            }
        }
    }

    public bool IsKindChanged
    {
        get => _IsKindChanged;
        internal set => SetProperty(ref _IsKindChanged, value);
    }

    #endregion Kind

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

    #region Start

    private DateTime? _CurrentStart;
    private DateTime? _NewStart;
    private bool _IsStartChanged;

    public DateTime? CurrentStart
    {
        get => _CurrentStart;
        internal set
        {
            if (SetProperty(ref _CurrentStart, value?.Date))
            {
                NewStart = _CurrentStart;
                IsStartChanged = _NewStart != _CurrentStart;
            }
        }
    }

    public DateTime? NewStart
    {
        get => _NewStart;
        set
        {
            if (SetProperty(ref _NewStart, value?.Date))
            {
                IsStartChanged = _NewStart != _CurrentStart;
            }
        }
    }

    public bool IsStartChanged
    {
        get => _IsStartChanged;
        internal set => SetProperty(ref _IsStartChanged, value);
    }

    #endregion Start

    #region End

    private DateTime? _CurrentEnd;
    private DateTime? _NewEnd;
    private bool _IsEndChanged;

    public DateTime? CurrentEnd
    {
        get => _CurrentEnd;
        internal set
        {
            if (SetProperty(ref _CurrentEnd, value?.Date))
            {
                NewEnd = _CurrentEnd;
                IsEndChanged = _NewEnd != _CurrentEnd;
            }
        }
    }

    public DateTime? NewEnd
    {
        get => _NewEnd;
        set
        {
            if (SetProperty(ref _NewEnd, value?.Date))
            {
                IsEndChanged = _NewEnd != _CurrentEnd;
            }
        }
    }

    public bool IsEndChanged
    {
        get => _IsEndChanged;
        internal set => SetProperty(ref _IsEndChanged, value);
    }

    #endregion End

    #region Image

    private string _CurrentImage = string.Empty;
    private string _NewImage = string.Empty;
    private bool _IsImageChanged;

    public string CurrentImage
    {
        get => _CurrentImage;
        internal set
        {
            if (SetProperty(ref _CurrentImage, value?.Trim() ?? string.Empty))
            {
                NewImage = _CurrentImage;
                IsImageChanged = _NewImage != _CurrentImage;
            }
        }
    }

    public string NewImage
    {
        get => _NewImage;
        set
        {
            if (SetProperty(ref _NewImage, value?.Trim() ?? string.Empty))
            {
                IsImageChanged = _NewImage != _CurrentImage;
            }
        }
    }

    public bool IsImageChanged
    {
        get => _IsImageChanged;
        internal set => SetProperty(ref _IsImageChanged, value);
    }

    #endregion Image

    #region Item1

    private ItemViewModel? _Item1;

    public ItemViewModel? Item1
    {
        get => _Item1;
        internal set
        {
            if (SetProperty(ref _Item1, value))
            {
                Items = null;
            }
        }
    }

    #endregion Item1

    #region Item2

    private ItemViewModel? _Item2;

    public ItemViewModel? Item2
    {
        get => _Item2;
        internal set
        {
            if (SetProperty(ref _Item2, value))
            {
                Items = null;
            }
        }
    }

    #endregion Item2

    #region Item3

    private ItemViewModel? _Item3;

    public ItemViewModel? Item3
    {
        get => _Item3;
        internal set
        {
            if (SetProperty(ref _Item3, value))
            {
                Items = null;
            }
        }
    }

    #endregion Item3

    #region Item4

    private ItemViewModel? _Item4;

    public ItemViewModel? Item4
    {
        get => _Item4;
        internal set
        {
            if (SetProperty(ref _Item4, value))
            {
                Items = null;
            }
        }
    }

    #endregion Item4

    private ReadOnlyCollection<ItemViewModel?>? _Items;

    public ReadOnlyCollection<ItemViewModel?> Items
    {
        get => _Items ??= new([_Item1, _Item2, _Item3, _Item4]);
        private set => SetProperty(ref _Items, value);
    }

    internal void AddItem(ItemViewModel item)
    {
        if (_Item1 == null)
        {
            Item1 = item;
        }
        else if (_Item2 == null)
        {
            Item2 = item;
        }
        else if (_Item3 == null)
        {
            Item3 = item;
        }
        else if (_Item4 == null)
        {
            Item4 = item;
        }
    }
}
