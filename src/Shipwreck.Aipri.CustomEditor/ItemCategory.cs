using System.ComponentModel.DataAnnotations;

namespace Shipwreck.Aipri.CustomEditor;

public enum ItemCategory
{
    [Display(Name = "トップス")]
    Tops,

    [Display(Name = "ワンピ")]
    OnePiece,

    [Display(Name = "ボトムス")]
    Bottoms,

    [Display(Name = "シューズ")]
    Shoes,

    [Display(Name = "アクセ")]
    Accessory,
}
