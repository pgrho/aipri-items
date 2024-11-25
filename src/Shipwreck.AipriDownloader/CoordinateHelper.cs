namespace Shipwreck.AipriDownloader;

internal static class CoordinateHelper
{
    public static void BeginSetImageUrl(this Coordinate c, string? imageUrl, DownloadContext dc)
    {
        if (c.LoadingImageUrl == imageUrl && c.ImageTask != null)
        {
            return;
        }

        c.LoadingImageUrl = imageUrl;
        c.ImageTask = dc.GetOrCopyImageAsync(imageUrl, "coordinates", c.Id);
        c.ImageTask!.ContinueWith(t => c.ImageUrl = t.Status == TaskStatus.RanToCompletion ? t.Result : c.ImageUrl);
    }

    public static void BeginSetThumbnailUrl(this Coordinate c, string? thumbnailUrl, DownloadContext dc)
    {
        if (c.LoadingThumbnailUrl == thumbnailUrl && c.ThumbnailTask != null)
        {
            return;
        }

        c.LoadingThumbnailUrl = thumbnailUrl;
        c.ThumbnailTask = dc.GetOrCopyImageAsync(thumbnailUrl, "coordinates", c.Id, "-thumb");
        c.ThumbnailTask!.ContinueWith(t => c.ThumbnailUrl = t.Status == TaskStatus.RanToCompletion ? t.Result : c.ThumbnailUrl);
    }
}
