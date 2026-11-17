using System.Buffers.Binary;
using System.Text;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class FontPrefixStorageTests
{
    private static byte[] Input(bool longLocations = true) => AuthoredTrueType.Build(longLocations: longLocations,
        tableOverrides: new Dictionary<string, byte[]> { ["zzzz"] = new byte[131072] });
    private static int Prefix(byte[] data) => AuthoredTrueType.Offset(data, "maxp") + 32;
    private static int Entry(byte[] data, string tag)
    {
        var encoded = Encoding.ASCII.GetBytes(tag);
        for (var p = 12; p < 12 + BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(4)) * 16; p += 16)
            if (data.AsSpan(p, 4).SequenceEqual(encoded)) return p;
        throw new Exception("Missing authored table.");
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ActualOwnedPrefixIsAdmittedAndIndependentUntilFinalDisposal(bool longLocations)
    {
        var input = Input(longLocations); var saved = input.ToArray(); var prefix = Prefix(input);
        var capacity = prefix + 256L; var pool = new PixelStoragePool();
        var idle = new byte[prefix]; idle.AsSpan().Fill(0x52); pool.ReturnLatest(idle);
        using var root = new TestMemoryBudget(2 * capacity);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = input.Length + capacity };
        using var first = TrueTypeFont.LoadCore(input, limits, default, pool);
        using var second = TrueTypeFont.LoadCore(input, limits, default, pool);
        Assert.Equal(capacity, first.StorageBytes); Assert.Equal(2 * capacity, root.Bytes);
        Assert.Equal(2 * capacity, root.Peak); Assert.Equal(saved, input);
        var refused = Assert.Throws<ImageLimitException>(() => TrueTypeFont.LoadCore(input, limits, default, pool));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope); Assert.Equal(1, root.Refusals);
        input.AsSpan().Clear();
        Assert.Equal(600, first.GetGlyphMetrics(new Rune('A')).AdvanceWidth);
        first.Dispose(); Assert.Equal(capacity, root.Bytes);
        Assert.Equal(600, second.GetGlyphMetrics(new Rune('A')).AdvanceWidth);
        using (var recovered = TrueTypeFont.LoadCore(saved, limits, default, pool)) Assert.Equal(2 * capacity, root.Bytes);
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        var dirty = pool.RentPreservingIdle(prefix);
        Assert.Same(idle, dirty); Assert.All(dirty, value => Assert.Equal(0x52, value));
    }

    [Theory]
    [InlineData("GDEF")] [InlineData("GPOS")] [InlineData("GSUB")]
    [InlineData("kern")] [InlineData("vhea")] [InlineData("vmtx")]
    public void LazyConsumersKeepTheirCompleteIndependentlyOwnedTables(string tag)
    {
        var bytes = Enumerable.Range(0, 1027).Select(i => (byte)(i * 19)).ToArray();
        var input = AuthoredTrueType.Build(tableOverrides: new Dictionary<string, byte[]> { [tag] = bytes, ["zzzz"] = new byte[8192] });
        using var font = TrueTypeFont.Load(input);
        input.AsSpan().Clear();
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.True(font.TableForText(Encoding.ASCII.GetBytes(tag), context).Span.SequenceEqual(bytes));
    }

    [Theory]
    [InlineData("name")] [InlineData("post")] [InlineData("zzzz")]
    public void DiscardedDirectoryRangesStillFailBeforeLaterMetricErrors(string tag)
    {
        var input = Input(); var clean = input.ToArray();
        AuthoredTrueType.W32(input, Entry(input, tag) + 12, uint.MaxValue);
        AuthoredTrueType.W32(input, AuthoredTrueType.Offset(input, "head") + 12, 0);
        using var root = new TestMemoryBudget(input.Length + 256L);
        var limits = new ImageLimits { MemoryBudget = root };
        var failure = Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(input, limits));
        Assert.Equal("Invalid font table range or ordering.", failure.Message);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using (var recovered = TrueTypeFont.Load(clean, limits)) Assert.Equal(Prefix(clean) + 256L, root.Bytes);
        Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void UnknownTrailingBytesDoNotBypassInputOrContainerLimits()
    {
        var input = Input();
        var failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.Load(input,
            new ImageLimits { MaxInputBytes = input.Length - 1 }));
        Assert.Equal(ImageLimitKind.InputBytes, failure.Limit);
        failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.Load(input,
            new ImageLimits { MaxContainerItems = 1 }));
        Assert.Equal(ImageLimitKind.ContainerItems, failure.Limit);
    }

    [Fact]
    public void CancellationAfterActualAdmissionLeavesIdleStorageUntouchedAndRecovers()
    {
        var input = Input(); var prefix = Prefix(input); var capacity = prefix + 256L;
        var pool = new PixelStoragePool(); var idle = new byte[prefix]; idle.AsSpan().Fill(0x36); pool.ReturnLatest(idle);
        using var cancellation = new CancellationTokenSource(); using var root = new TestMemoryBudget(capacity);
        root.Admitted = _ => cancellation.Cancel();
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = input.Length + capacity };
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => TrueTypeFont.LoadCore(input, limits, cancellation.Token, pool));
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(0, root.Bytes);
        Assert.Same(idle, pool.RentPreservingIdle(prefix)); Assert.All(idle, value => Assert.Equal(0x36, value));
        root.Admitted = null;
        using (var recovered = TrueTypeFont.LoadCore(input, limits, default, pool)) Assert.Equal(capacity, root.Bytes);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RetainedLayoutSurvivesFontDisposalAndExactPrefixReuse()
    {
        var input = Input(); var prefix = Prefix(input); var pool = new PixelStoragePool();
        var font = TrueTypeFont.LoadCore(input, null, default, pool);
        using var layout = TextLayout.Create(font, "Ag A\u0301", new TextOptions { FontSize = 20 });
        using var before = ImageBuffer.Create(100, 40, 16);
        ImageDrawing.DrawText(before, layout, 1, 1, RgbaColor.From8(123, 234, 45));
        font.Dispose();
        var released = pool.RentPreservingIdle(prefix); Assert.Equal(input[..prefix], released);
        released.AsSpan().Clear(); pool.ReturnLatest(released);
        using var next = TrueTypeFont.LoadCore(input, null, default, pool);
        using var after = ImageBuffer.Create(100, 40, 16);
        ImageDrawing.DrawText(after, layout, 1, 1, RgbaColor.From8(123, 234, 45));
        Assert.True(before.Pixels.SequenceEqual(after.Pixels));
    }
}
