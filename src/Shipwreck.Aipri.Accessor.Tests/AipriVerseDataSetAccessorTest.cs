using System.IO;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Shipwreck.Aipri.Accessor;

public class AipriVerseDataSetAccessorTest
{
    private readonly ITestOutputHelper _Output;

    public AipriVerseDataSetAccessorTest(ITestOutputHelper output)
    {
        _Output = output;
    }

    [Fact]
    public async Task Test()
    {
        using var a = new AipriVerseDataSetAccessor(Path.Combine(Path.GetTempPath(), GetType().Namespace!, GetType().Name));
        var ds = await a.GetAsync();

        foreach (var b in ds.Brands)
        {
            using var fs = b.OpenImage();
            _Output.WriteLine($"{b.GetType().FullName}: {fs.Length}");
        }
        foreach (var b in ds.Coordinates)
        {
            if (b.ImageUrl != null)
            {
                using var fs = b.OpenImage();
                _Output.WriteLine($"{b.GetType().FullName}: {fs.Length}");
            }

            if (b.ThumbnailUrl != null)
            {
                using var th = b.OpenThumbnail();
                _Output.WriteLine($"{b.GetType().FullName}: {th.Length}");
            }
        }
        foreach (var b in ds.CoordinateItems)
        {
            using var fs = b.OpenImage();
            _Output.WriteLine($"{b.GetType().FullName}: {fs.Length}");
        }
    }
}