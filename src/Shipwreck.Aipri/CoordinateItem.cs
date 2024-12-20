﻿using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shipwreck.Aipri;

public sealed class CoordinateItem : DataItem
{
    public int Id { get; set; }

    public int CoordinateId { get; set; }

    public Coordinate? GetCoordinate() => DataSet?.Coordinates.GetById(CoordinateId);

    public string? SealId { get; set; }

    public int CategoryId { get; set; }

    public Category? GetCategory() => DataSet?.Categories.GetById(CategoryId);

    public short Point { get; set; }
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    internal string? LoadingImageUrl { get; set; }

    [JsonIgnore]
    internal Task<string?>? ImageTask { get; set; }

    [JsonIgnore]
    internal bool IsCurrentRun { get; set; }

    public CoordinateItem Clone()
        => new()
        {
            Id = Id,
            CoordinateId = CoordinateId,
            SealId = SealId,
            CategoryId = CategoryId,
            Point = Point,
            ImageUrl = ImageUrl
        };
}