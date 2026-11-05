using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class TextActiveEdgeTests
{
    [Theory]
    [InlineData(8, "6b5edd6194c73a9b48b8d26a3fb68607f26811d17bf2cb733e394cb6b2d79aaa")]
    [InlineData(16, "5d117ab0d9306ee17e0abc631ffffd68809edf6694d21eebdd27f350c1f65589")]
    public void InterleavedCoincidentContoursKeepTheOriginalClippedRepeatedFrame(int depth, string expected)
    {
        // Frozen through the qualified original public DLL, before active-edge
        // scheduling. Curves, cancelling contours, repeated glyphs and clipped
        // starts put activation and original winding/tie orders in conflict.
        using var root = new TestMemoryBudget(2_000_000);
        var actual = ActiveEdgeFixture.Render(depth, new() { MemoryBudget = root });
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(actual)));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void RetainedRecordsFitTheirExistingPerItemAdmission()
    {
        // Physical storage changes with the retained original edge identity.
        // Both arrays must still fit the capacity admitted before allocation.
        var flags = BindingFlags.NonPublic;
        Assert.Equal(40, Marshal.SizeOf(typeof(TextRasterizer).GetNestedType("Edge", flags)!));
        Assert.Equal(16, Marshal.SizeOf(typeof(TextRasterizer).GetNestedType("Crossing", flags)!));
    }
}
