namespace Shipwreck.AipriDownloader;

public sealed class CorrectionData
{
    public CorrectionEntry<Coordinate>[] Coordinates { get; set; } = [];
    public CorrectionEntry<CoordinateItem>[] CoordinateItems { get; set; } = [];
}
