using System.Text.Json.Serialization;

namespace Shipwreck.Aipri;

public abstract class DataItem
{
    [JsonIgnore]
    public AipriDataSet? DataSet { get; internal set; }
}
