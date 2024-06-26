﻿using System.Text.Json.Serialization;

namespace Shipwreck.Aipri.Accessor;

public sealed class AipriGitDataSet : AipriDataSet
{
    [JsonIgnore]
    public string? FileName { get; internal set; }
}