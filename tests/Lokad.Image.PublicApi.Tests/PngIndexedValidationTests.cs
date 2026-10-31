using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngIndexedValidationTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CertifiedIndexedRowsKeepDescriptorsTruncationsAndOriginalFailureOrder(bool interlace)
    {
        var frame = new AuthoredPng.Frame("indexed-validation", 3, 4, 19, 17, interlace);
        var raw = AuthoredPng.FilteredRows(frame);
        foreach (var compressed in new[]
        {
            AuthoredPng.StoredZlib(raw),
            AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw)
        })
        {
            var encoded = AuthoredPng.Wrap(frame, compressed); var original = encoded.ToArray();
            using (var image = ImageCodec.Decode(encoded, ReferenceOptions(frame, encoded)))
                Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(image));
            Assert.Equal(ImageCodec.Identify(encoded), ImageCodec.ValidatePixels(encoded));
            using (var token = new CancellationTokenSource())
                Assert.Equal(ImageCodec.Identify(encoded), ImageCodec.ValidatePixels(encoded, cancellationToken: token.Token));
            for (var length = 8; length < encoded.Length; length++) Compare(encoded[..length], frame);
            for (var length = 0; length < compressed.Length; length++) Compare(AuthoredPng.Wrap(frame, compressed[..length]), frame);
            var badChecksum = compressed.ToArray(); badChecksum[^1] ^= 1;
            Compare(AuthoredPng.Wrap(frame, badChecksum), frame);
            var badGrammar = compressed.ToArray(); badGrammar[2] = 7;
            Compare(AuthoredPng.Wrap(frame, badGrammar), frame);
            Compare(SingleColorPalette(encoded), frame);
            Assert.Equal(original, encoded);
        }
        foreach (var lastRow in new[] { false, true })
        foreach (var badChecksum in new[] { false, true })
        {
            var invalid = raw.ToArray(); invalid[lastRow ? invalid.Length - 11 : 0] = 5;
            var compressed = AuthoredDeflate.Fixed(invalid.Select(value => new AuthoredDeflate.Token(value)), invalid);
            if (badChecksum) compressed[^1] ^= 1;
            Compare(AuthoredPng.Wrap(frame, compressed), frame);
        }
        var extra = raw.Concat(new byte[] { 0 }).ToArray();
        Compare(AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(extra)), frame);
    }

    [Fact]
    public void IndexedScratchSharesAggregateAdmissionAndCleansEveryRefusalAndCancellation()
    {
        var frame = new AuthoredPng.Frame("indexed-admission", 3, 4, 19, 17, true);
        var input = AuthoredPng.Build(frame); var original = input.ToArray(); var expected = ImageCodec.Identify(input);
        using var reference = new TestMemoryBudget(1024 * 1024);
        Assert.Equal(expected, ImageCodec.ValidatePixels(input, new() { MemoryBudget = reference }));
        Assert.Equal(0, reference.Bytes); Assert.False(reference.IsDisposed);
        Assert.InRange(reference.Peak, 4096, 64 * 1024);
        var growths = reference.GrowthAttempts; Assert.InRange(growths, 1, 64);
        for (var failure = 1; failure <= growths; failure++)
        {
            using var root = new TestMemoryBudget(1024 * 1024) { RefuseAtGrowth = failure };
            var limits = new ImageLimits { MemoryBudget = root };
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(input, limits));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            Assert.Equal(1, root.Refusals); Assert.Equal(failure, root.GrowthAttempts); Assert.Equal(0, root.Bytes);
            root.RefuseAtGrowth = 0;
            Assert.Equal(expected, ImageCodec.ValidatePixels(input, limits));
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        for (var growth = 1; growth <= growths; growth++)
        {
            using var root = new TestMemoryBudget(1024 * 1024); using var token = new CancellationTokenSource();
            root.Admitted = _ => { if (root.GrowthAttempts == growth) token.Cancel(); };
            var limits = new ImageLimits { MemoryBudget = root };
            var error = Assert.Throws<OperationCanceledException>(() => ImageCodec.ValidatePixels(input, limits, token.Token));
            Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            root.Admitted = null;
            Assert.Equal(expected, ImageCodec.ValidatePixels(input, limits)); Assert.Equal(0, root.Bytes);
        }
        using var shared = new TestMemoryBudget(reference.Peak); var entered = false;
        var sharedLimits = new ImageLimits { MemoryBudget = shared };
        shared.Admitted = used =>
        {
            if (entered || used != reference.Peak) return;
            entered = true;
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(input, sharedLimits));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(used, shared.Bytes);
        };
        Assert.Equal(expected, ImageCodec.ValidatePixels(input, sharedLimits));
        Assert.True(entered); Assert.Equal(1, shared.Refusals); Assert.Equal(0, shared.Bytes);
        shared.Admitted = null;
        Assert.Equal(expected, ImageCodec.ValidatePixels(input, sharedLimits));
        Assert.Equal(0, shared.Bytes); Assert.False(shared.IsDisposed); Assert.Equal(original, input);
    }

    [Fact]
    public void ValidIncompleteTreesKeepTheirBclAcceptanceAndReleaseSpeculativeStorage()
    {
        var frame = new AuthoredPng.Frame("indexed-incomplete", 3, 4, 2, 1, false);
        byte[] raw = [0, 17];
        var encoded = AuthoredPng.Wrap(frame, AuthoredDeflate.DynamicLiteral(raw));
        Compare(encoded, frame);
        using var root = new TestMemoryBudget(1024 * 1024);
        Assert.Equal(ImageCodec.Identify(encoded), ImageCodec.ValidatePixels(encoded, new() { MemoryBudget = root }));
        Assert.True(root.Peak >= 256 * 1024); // The incomplete distance tree uses the original inflater.
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LargerAndTightWorkingLimitsKeepBoundedOriginalPaths()
    {
        var frame = new AuthoredPng.Frame("indexed-over-cap", 3, 4, 1024, 512, false);
        var encoded = AuthoredPng.Build(frame); var original = encoded.ToArray();
        using var root = new TestMemoryBudget(1024 * 1024);
        Assert.Equal(ImageCodec.Identify(encoded), ImageCodec.ValidatePixels(encoded, new() { MemoryBudget = root }));
        Assert.InRange(root.Peak, 256 * 1024, 1024 * 1024);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(original, encoded);
        frame = new("indexed-tight", 3, 4, 19, 17, true);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)); encoded = AuthoredPng.Wrap(frame, compressed);
        var oldPeak = encoded.Length + (long)compressed.Length + 56 + 256 * 1024 + 20;
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(encoded,
            new() { MemoryBudget = root, MaxWorkingBytes = oldPeak - 1 }));
        Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.Operation, error.Scope);
        Assert.Equal(0, root.Bytes);
        Assert.Equal(ImageCodec.Identify(encoded), ImageCodec.ValidatePixels(encoded, new() { MemoryBudget = root }));
        Assert.Equal(0, root.Bytes);
    }

    private static DecodeOptions ReferenceOptions(AuthoredPng.Frame frame, byte[] input)
        // Below the existing materialization threshold: Decode uses the original
        // BCL inflater. This is independent of the new validation storage path.
        => new() { Limits = new() { MaxWorkingBytes = input.Length * 2L + 256 * 1024
            + (long)frame.Width * frame.Height * 4 + ((frame.Width * frame.Depth + 7) / 8) * 2 + 4096 } };

    private static void Compare(byte[] input, AuthoredPng.Frame frame)
    {
        var original = input.ToArray();
        var expected = Record.Exception(() => { using var image = ImageCodec.Decode(input, ReferenceOptions(frame, input)); });
        ImageInfo? info = null;
        var actual = Record.Exception(() => info = ImageCodec.ValidatePixels(input));
        Assert.Equal(expected?.GetType(), actual?.GetType()); Assert.Equal(expected?.Message, actual?.Message);
        if (expected is null) Assert.Equal(ImageCodec.Identify(input), info);
        Assert.Equal(original, input);
    }

    private static byte[] SingleColorPalette(byte[] input)
    {
        using var result = new MemoryStream(); result.Write(input.AsSpan(0, 8));
        for (var offset = 8; offset < input.Length;)
        {
            var length = (int)BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(offset));
            var type = System.Text.Encoding.ASCII.GetString(input.AsSpan(offset + 4, 4));
            var data = input.AsSpan(offset + 8, length);
            if (type == "PLTE") data = data[..3];
            if (type == "tRNS") data = data[..1];
            AuthoredPng.Chunk(result, type, data); offset += length + 12;
        }
        return result.ToArray();
    }
}
