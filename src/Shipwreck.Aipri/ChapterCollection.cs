﻿namespace Shipwreck.Aipri;

public sealed class ChapterCollection : DataItemCollection<Chapter>
{
    public ChapterCollection()
        : base(null) { }

    public ChapterCollection(IEnumerable<Chapter> items)
        : base(null)
    {
        Set(items);
    }

    internal ChapterCollection(AipriDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Chapter? GetById(string id)
        => this.FirstOrDefault(e => e.Id == id);
}
