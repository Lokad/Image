using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class TextRasterizerTests
{
    private static TextLayout Rectangle()
    {
        var outline = AuthoredTrueType.Outline([[(0, 0, true), (0, 600, true), (1000, 600, true), (1000, 0, true)]]);
        return TextLayout.Create(TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]> { [2] = outline })), "A", new TextOptions { FontSize = 10 });
    }
    [Fact]
    public void ExactPeakAdmissionCountsSubdivisionStackAndReleasesEveryLease()
    {
        var layout = Rectangle(); var context = new OperationContext(new() { MaxWorkingBytes = 6560 }, default);
        var raster = new TextRasterizer(layout, context, 10); Assert.Equal(1328, context.RemainingWorkingBytes);
        raster.Dispose(); raster.Dispose(); Assert.Equal(6560, context.RemainingWorkingBytes);
        var denied = new OperationContext(new() { MaxWorkingBytes = 6559 }, default);
        var failure = Assert.Throws<ImageLimitException>(() => new TextRasterizer(layout, denied, 10));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(6559, denied.RemainingWorkingBytes);
    }
}
