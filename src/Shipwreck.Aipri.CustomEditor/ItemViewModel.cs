using Shipwreck.ViewModelUtils;

namespace Shipwreck.Aipri.CustomEditor;

public sealed class ItemViewModel : ObservableModel
{
    #region Category

    private ItemCategory _CurrentCategory;
    private ItemCategory _NewCategory;
    private bool _IsCategoryChanged;

    public ItemCategory CurrentCategory
    {
        get => _CurrentCategory;
        internal set
        {
            if (SetProperty(ref _CurrentCategory, value))
            {
                NewCategory = _CurrentCategory;
                IsCategoryChanged = _NewCategory != _CurrentCategory;
            }
        }
    }

    public ItemCategory NewCategory
    {
        get => _NewCategory;
        set
        {
            if (SetProperty(ref _NewCategory, value))
            {
                IsCategoryChanged = _NewCategory != _CurrentCategory;
            }
        }
    }

    public bool IsCategoryChanged
    {
        get => _IsCategoryChanged;
        internal set => SetProperty(ref _IsCategoryChanged, value);
    }

    #endregion Category

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

    #region Set

    private bool _CurrentSet;
    private bool _NewSet;
    private bool _IsSetChanged;

    public bool CurrentSet
    {
        get => _CurrentSet;
        internal set
        {
            if (SetProperty(ref _CurrentSet, value))
            {
                NewSet = _CurrentSet;
                IsSetChanged = _NewSet != _CurrentSet;
            }
        }
    }

    public bool NewSet
    {
        get => _NewSet;
        set
        {
            if (SetProperty(ref _NewSet, value))
            {
                IsSetChanged = _NewSet != _CurrentSet;
            }
        }
    }

    public bool IsSetChanged
    {
        get => _IsSetChanged;
        internal set => SetProperty(ref _IsSetChanged, value);
    }

    #endregion Set
}
