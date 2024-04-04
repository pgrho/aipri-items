using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public abstract class DataItem
{
    [JsonIgnore]
    public AipriVerseDataSet? DataSet { get; internal set; }
}
