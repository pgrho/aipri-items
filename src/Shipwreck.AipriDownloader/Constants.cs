namespace Shipwreck.AipriDownloader;

internal static class Constants
{
    public const string BRANDS = "brands";
    public const string BRAND_PATH_FORMAT = BRANDS + "/{0:D6}{1}";

    public const string COORDINATES = "coordinates";
    public const string COORDINATE_PATH_FORMAT = COORDINATES + "/{0:D6}{1}";

    public const string COORDINATE_ITEMS = "coordinateItems";
    public const string COORDINATE_ITEM_PATH_FORMAT = COORDINATE_ITEMS + "/{0:D6}{1}";

    public const string COORDINATE_THUMBNAIL_PATH_FORMAT = COORDINATES + "/{0:D6}-thumb{1}";

    public const string CARDS = "cards";
    public const string CARD_PATH_FORMAT1 = CARDS + "/{0:D6}-1{1}";
    public const string CARD_PATH_FORMAT2 = CARDS + "/{0:D6}-2{1}";
}