﻿namespace Shipwreck.Aipri;

public sealed class BrandCollection : DataItemCollection<Brand>
{
    public BrandCollection()
        : base(null) { }

    public BrandCollection(IEnumerable<Brand> items)
        : base(null)
    {
        Set(items);
    }

    internal BrandCollection(AipriDataSet? dataSet)
        : base(dataSet)
    {
    }

    // TODO index
    public Brand? GetByName(string name)
        => this.FirstOrDefault(e => e.Name == name);
}
