using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosPlanTests
{
    // Authored plans captured from the qualified implementation on both OSes.
    // Hash covers dimensions, every window/offset and every signed Q30 weight.
    [Theory]
    [InlineData(513, 256, "6352ef87d72ced341b5267bce122dee82cea4771182db2f7fd4c559d79be746a")]
    [InlineData(259, 129, "610dd92d5bccdf95bfb35b6988f8d71246bd7932701b9209c6f304492ea91252")]
    [InlineData(513, 1027, "6ac9d19c56da5ab06969c96bab30775183221fb4a10a94d2f2a936dc93735125")]
    [InlineData(259, 519, "335d6b86ea48568f8abf639e20130381ef69c2ba04e526328e20731d8b381edd")]
    [InlineData(31, 9, "fae939517b9416009df42384f653f913c98e0ad6735c618970573862daecf467")]
    [InlineData(1, 5, "25b725523ab7faacc80518a3f1e3f5842cd5270cca5f8fad24ec046f8ee46115")]
    [InlineData(5, 1, "1a82d984ac230a2dc7c4d89f53b920e7752037b2c9a430f4cf8c82c7138cc59d")]
    [InlineData(1031, 2, "aa2497407cebee365179e5f08ee12c7109686cc15fd49abb5a6fc897a8f7fa39")]
    public void ExactPlansKeepOriginalCoefficientBitsAndOwnership(int source, int target, string expected)
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var remaining = context.RemainingWorkingBytes;
        using (var plan = LanczosPlan.Create(source, target, context))
        {
            using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
            writer.Write(source); writer.Write(target); writer.Write(plan.Windows.Length);
            foreach (var w in plan.Windows) { writer.Write(w.First); writer.Write(w.Count); writer.Write(w.Offset); }
            writer.Write(plan.Weights.Length); foreach (var w in plan.Weights) writer.Write(w);
            writer.Flush();
            Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(stream.ToArray())));
        }
        Assert.Equal(remaining, context.RemainingWorkingBytes);
    }

    [Fact]
    public void InteriorMirrorsAndClampedEdgesMatchIndependentDecimalAxis()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var plan = LanczosPlan.Create(31, 9, context);
        var expected = LanczosReference.Axis(31, 9);
        for (var x = 0; x < 9; x++)
        for (var sample = 0; sample < 31; sample++)
        {
            var w = plan.Windows[x];
            var actual = sample >= w.First && sample < w.First + w.Count
                ? plan.Weights[w.Offset + sample - w.First] : 0;
            Assert.Equal(expected[x][sample], actual);
        }
    }
}
