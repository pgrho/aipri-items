namespace Shipwreck.AipriDownloader;

internal static class CardHelper
{
    public static void BeginSetImage1Url(this Card c, string? image1Url, DownloadContext dc)
    {
        if (c.LoadingImage1Url == image1Url && c.Image1Task != null)
        {
            return;
        }

        c.LoadingImage1Url = image1Url;
        c.Image1Task = dc.GetOrCopyImageAsync(image1Url, "cards", c.Id, "-1");
        c.Image1Task!.ContinueWith(t => c.Image1Url = t.Status == TaskStatus.RanToCompletion ? t.Result : c.Image1Url);
    }

    public static void BeginSetImage2Url(this Card c, string? image2Url, DownloadContext dc)
    {
        if (c.LoadingImage2Url == image2Url && c.Image2Task != null)
        {
            return;
        }

        c.LoadingImage2Url = image2Url;
        c.Image2Task = dc.GetOrCopyImageAsync(image2Url, "cards", c.Id, "-2");
        c.Image2Task!.ContinueWith(t => c.Image2Url = t.Status == TaskStatus.RanToCompletion ? t.Result : c.Image2Url);
    }
}