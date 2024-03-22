using System.Text.Json.Serialization;

namespace Shipwreck.AipriDownloader;

public abstract class DataItem
{
    [JsonIgnore]
    public AipriVerseData? DataSet { get; internal set; }
}
