﻿using System;
using System.IO;
using Shipwreck.AipriDownloader;

namespace Shipwreck.Aipri.Accessor;

public static partial class AipriGitDataSetHelper
{
    #region Brand

    public static string? GetImagePath(this Brand? e)
        => GetFilePath(Constants.BRAND_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Uri? GetImageUri(this Brand? e)
        => GetUri(Constants.BRAND_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenImage(this Brand? e)
        => Open(Constants.BRAND_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    #endregion Brand

    public static string? GetImagePath(this Coordinate? e)
        => GetFilePath(Constants.COORDINATE_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Uri? GetImageUri(this Coordinate? e)
        => GetUri(Constants.COORDINATE_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenImage(this Coordinate? e)
        => Open(Constants.COORDINATE_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    public static string? GetThumbnailPath(this Coordinate? e)
        => GetFilePath(Constants.COORDINATE_THUMBNAIL_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ThumbnailUrl);

    public static Uri? GetThumbnailUri(this Coordinate? e)
        => GetUri(Constants.COORDINATE_THUMBNAIL_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ThumbnailUrl);

    public static Stream OpenThumbnail(this Coordinate? e)
        => Open(Constants.COORDINATE_THUMBNAIL_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -1, e?.ThumbnailUrl!);

    public static string? GetImagePath(this CoordinateItem? e)
        => GetFilePath(Constants.COORDINATE_ITEM_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Uri? GetImageUri(this CoordinateItem? e)
        => GetUri(Constants.COORDINATE_ITEM_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenImage(this CoordinateItem? e)
        => Open(Constants.COORDINATE_ITEM_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    public static string? GetImagePath(this Song? e)
        => GetFilePath("../custom/songs/{2}.jpg", (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.Name);

    public static Uri? GetImageUri(this Song? e)
        => GetUri("../custom/songs/{2}.jpg", (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.Name);

    public static Stream OpenImage(this Song? e)
        => Open("../custom/songs/{2}.jpg", (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -1, e?.Name!);

    #region Part

    public static string? GetImagePath(this Part? e)
        => GetFilePath(Constants.PART_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Uri? GetImageUri(this Part? e)
        => GetUri(Constants.PART_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.ImageUrl);

    public static Stream OpenImage(this Part? e)
        => Open(Constants.PART_PATH_FORMAT, (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -1, e?.ImageUrl!);

    #endregion Part

    #region Card

    #region Image1

    public static string? GetImage1Path(this Card? e)
        => GetFilePath(Constants.CARD_PATH_FORMAT1, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.Image1Url);

    public static Uri? GetImage1Uri(this Card? e)
        => GetUri(Constants.CARD_PATH_FORMAT1, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.Image1Url);

    public static Stream OpenImage1(this Card? e)
        => Open(Constants.CARD_PATH_FORMAT1, (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -1, e?.Image1Url!);

    #endregion Image1

    #region Image2

    public static string? GetImage2Path(this Card? e)
        => GetFilePath(Constants.CARD_PATH_FORMAT2, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.Image2Url);

    public static Uri? GetImage2Uri(this Card? e)
        => GetUri(Constants.CARD_PATH_FORMAT2, (e?.DataSet as AipriGitDataSet)?.FileName, e?.Id, e?.Image2Url);

    public static Stream OpenImage2(this Card? e)
        => Open(Constants.CARD_PATH_FORMAT2, (e?.DataSet as AipriGitDataSet)?.FileName!, e?.Id ?? -2, e?.Image2Url!);

    #endregion Image2

    #endregion Card

    private static string? GetFilePath(string pathFormat, string? jsonPath, int? imageId, string? imageUrl)
    {
        if (imageUrl == null)
        {
            return null;
        }

        if (jsonPath != null)
        {
            var ju = new Uri(jsonPath);
            if (!imageUrl.StartsWith("http")
                && !pathFormat.Contains("{2}"))
            {
                return new Uri(new Uri(ju, "../custom/1"), imageUrl).LocalPath;
            }

            if (imageId > 0)
            {
                return new Uri(ju, string.Format(pathFormat, imageId, Path.GetExtension(imageUrl), imageUrl)).LocalPath;
            }
        }

        return null;
    }

    private static Uri? GetUri(string pathFormat, string? jsonPath, int? imageId, string? imageUrl)
    {
        if (imageUrl == null)
        {
            return null;
        }

        if (jsonPath != null)
        {
            var ju = new Uri(jsonPath);
            if (!imageUrl.StartsWith("http")
                && !pathFormat.Contains("{2}"))
            {
                return new Uri(new Uri(ju, "../custom/1"), imageUrl);
            }

            if (imageId > 0)
            {
                return new Uri(ju, string.Format(pathFormat, imageId, Path.GetExtension(imageUrl), imageUrl));
            }
        }

        return new Uri(imageUrl);
    }

    private static Stream Open(string pathFormat, string jsonPath, int imageId, string imageUrl)
        => new FileStream(
            GetFilePath(pathFormat, jsonPath, imageId, imageUrl) ?? throw new ArgumentException(),
            FileMode.Open, FileAccess.Read, FileShare.Read);
}