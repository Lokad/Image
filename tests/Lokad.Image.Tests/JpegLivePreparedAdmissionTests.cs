using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class JpegBaselineBlockTests
{
    [Theory]
    [InlineData(9)] [InlineData(10)]
    public void LiveDcAndLongAcKeepCanonicalFollowingReads(int length)
    {
        using var tables = Context();
        var dcCounts = new byte[16]; dcCounts[0] = 1;
        var acCounts = new byte[16]; acCounts[0] = 1; acCounts[length - 1] = 1;
        var dc = new JpegHuffman(dcCounts, [0], tables);
        var ac = new JpegHuffman(acCounts, [0, 7], tables);
        for (var phase = 0; phase < 8; phase++)
        {
            var data = Stuff(new string('0', phase) + "0" + "1" + new string('0', length - 1)
                + "1111111" + "0" + "10110100101" + new string('0', 32));
            using var live = new CancellationTokenSource();
            using var context = new OperationContext(ImageLimits.Default, live.Token);
            var reader = new JpegEntropyReader(data, 0, context); var original = reader;
            if (phase != 0) { reader.ReadBits(phase); original.ReadBits(phase); }
            var expected = Enumerable.Repeat(12345, 66).ToArray(); var actual = expected.ToArray();
            var predictor = 17; var originalPredictor = 17; var coefficient = 1;
            ScalarBaseline(ref original, expected.AsSpan(1, 64), ref originalPredictor, dc, ac, context);
            Assert.True(dc.TryReadCancelableBaselineBlock(ref reader, ac, actual.AsSpan(1, 64), ref predictor, ref coefficient));
            Assert.Equal(64, coefficient); Assert.Equal(originalPredictor, predictor);
            Assert.Equal(expected, actual); Assert.Equal(original.Position, reader.Position);
            Assert.Equal(original.ReadBits(11), reader.ReadBits(11));
            Assert.Equal(original.Position, reader.Position);
        }
    }
}

public sealed class JpegLivePreparedAdmissionTests
{
    [Fact]
    public void LiveScanPairRefusalAndCancellationReleaseBothTablesAndPreserveSurvivors()
    {
        // The encoder's 4:4:4 grid has two vertical blocks per MCU. Reach the
        // scan's preparation threshold without relying on a 1x1 sampling grid.
        const int width = 65, height = 129;
        using var source = OriginalPixels.Buffer(width, height, 8);
        using var encoded = new MemoryStream();
        ImageCodec.EncodeJpeg(source, encoded, new() { Quality = 85, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060 });
        var packet = encoded.ToArray(); var saved = packet.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        using var live = new CancellationTokenSource();
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        var pairs = new List<long>(); long previous = 0;
        root.Admitted = bytes =>
        {
            if (bytes - previous == JpegHuffman.BaselinePairBytes) pairs.Add(root.GrowthAttempts);
            previous = bytes;
        };
        using var first = ImageCodec.Decode(packet, options, live.Token);
        Assert.Equal(2, pairs.Count);
        var secondPairGrowth = pairs[1]; var retained = root.Bytes; var expected = OriginalPixels.Bytes(first);
        Assert.Equal(width * height * 4, retained);

        root.Admitted = null;
        root.RefuseAtGrowth = root.GrowthAttempts + secondPairGrowth;
        var refusedAt = root.RefuseAtGrowth;
        var refused = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(packet, options, live.Token));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
        Assert.Equal(refusedAt, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
        Assert.Equal(retained, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(first));
        root.RefuseAtGrowth = 0;

        using var canceled = new CancellationTokenSource();
        var cancelAt = root.GrowthAttempts + secondPairGrowth;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) canceled.Cancel(); };
        var error = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.Decode(packet, options, canceled.Token));
        Assert.Equal(canceled.Token, error.CancellationToken);
        Assert.Equal(cancelAt, root.GrowthAttempts); Assert.Equal(retained, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(first)); root.Admitted = null;

        using var recovered = ImageCodec.Decode(packet, options, live.Token);
        Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(recovered)); Assert.Equal(retained, root.Bytes);
        recovered.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(saved, packet);
    }
}
