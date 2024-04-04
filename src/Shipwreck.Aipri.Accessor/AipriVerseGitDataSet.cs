using System.Text.Json.Serialization;

namespace Shipwreck.Aipri.Accessor;

public sealed class AipriVerseGitDataSet : AipriVerseDataSet
{
    [JsonIgnore]
    public string? FileName { get; internal set; }
}
