namespace Shipwreck.AipriDownloader;

internal static class CoordinateItemHelper
{
    public static void BeginSetImageUrl(this CoordinateItem c, string? imageUrl, DownloadContext dc)
    {
        if (c.LoadingImageUrl == imageUrl && c.ImageTask != null)
        {
            return;
        }

        c.LoadingImageUrl = imageUrl;
        c.ImageTask = dc.GetOrCopyImageAsync(imageUrl, "coordinateItems", c.Id);
        c.ImageTask!.ContinueWith(t => c.ImageUrl = t.Status == TaskStatus.RanToCompletion ? t.Result : c.ImageUrl);
    }
}