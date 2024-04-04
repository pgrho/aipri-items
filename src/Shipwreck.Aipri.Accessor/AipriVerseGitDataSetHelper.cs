using System;
using System.IO;
using Shipwreck.AipriDownloader;

namespace Shipwreck.Aipri.Accessor;

public static class AipriVerseGitDataSetHelper
{
    public static string? GetImagePath(this Brand? e)
        => GetFilePath(Constants.BRAND_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenImage(this Brand? e)
        => Open(Constants.BRAND_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    public static string? GetImagePath(this Coordinate? e)
        => GetFilePath(Constants.COORDINATE_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenImage(this Coordinate? e)
        => Open(Constants.COORDINATE_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    public static string? GetThumbnailPath(this Coordinate? e)
        => GetFilePath(Constants.COORDINATE_THUMBNAIL_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenThumbnail(this Coordinate? e)
        => Open(Constants.COORDINATE_THUMBNAIL_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    public static string? GetImagePath(this CoordinateItem? e)
        => GetFilePath(Constants.COORDINATE_ITEM_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenImage(this CoordinateItem? e)
        => Open(Constants.COORDINATE_ITEM_PATH_FORMAT, (e?.DataSet as AipriVerseGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    private static string? GetFilePath(string pathFormat, string? jsonPath, int? imageId, string? imageUrl)
        => jsonPath != null && imageId != null && imageUrl != null
        ? new Uri(new Uri(jsonPath), string.Format(pathFormat, imageId, Path.GetExtension(imageUrl))).LocalPath : null;

    private static Stream Open(string pathFormat, string jsonPath, int imageId, string imageUrl)
        => new FileStream(
            GetFilePath(pathFormat, jsonPath, imageId, imageUrl) ?? throw new ArgumentException(),
            FileMode.Open, FileAccess.Read, FileShare.Read);
}