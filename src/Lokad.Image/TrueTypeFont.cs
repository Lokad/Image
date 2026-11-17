using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Runtime.CompilerServices;
using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Style supplied by the selected font face; no synthetic styling or system-font lookup.</summary>
[Flags]
public enum FontStyle { Regular = 0, Bold = 1, Italic = 2 }

/// <summary>Unscaled horizontal metrics and control-point bounds in font units. Empty glyphs have zero bounds.</summary>
public readonly record struct FontGlyphMetrics(int GlyphIndex, int AdvanceWidth, int LeftSideBearing,
    int XMin, int YMin, int XMax, int YMax, bool HasOutline);

/// <summary>An immutable, independently owned standalone TrueType face.</summary>
/// <remarks>Unicode cmap formats 4/12 and short/long loca are supported. Font loading
/// validates table ranges, selected mapping and glyph headers, not all outlines or
/// checksums. No font instructions are executed. Required source bytes are independently copied.</remarks>
public sealed partial class TrueTypeFont : IDisposable
{
    private const int DescriptorBytes = 256;
    private byte[] data;
    private OperationContext? storageOwner;
    private PixelStoragePool? storagePool;
    private bool disposed;
    private readonly int tableCount, metricCount, locationWidth, cmapFormat, mappingCount;
    private readonly TableRange glyf, loca, hmtx, cmap;
    private readonly record struct TableRange(int Offset, int Length);
    public int UnitsPerEm { get; }
    public int GlyphCount { get; }
    public int Ascender { get; }
    public int Descender { get; }
    public int LineGap { get; }
    public FontStyle Style { get; }
    /// <summary>Owned font bytes plus a coarse descriptor allowance, for caller lifetime reservations.</summary>
    public long StorageBytes => data.LongLength + DescriptorBytes;

    /// <summary>Copy and validate a supplied standalone TrueType face within finite admission limits.</summary>
    public static TrueTypeFont Load(ReadOnlySpan<byte> input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => LoadCore(input, limits, cancellationToken, PixelStoragePool.Shared);

    internal static TrueTypeFont LoadCore(ReadOnlySpan<byte> input, ImageLimits? limits,
        CancellationToken cancellationToken, PixelStoragePool pool)
    {
        var context = InputContext(input.Length, limits, cancellationToken);
        try
        {
            var retained = RetainedLength(input, context);
            context.Reserve(retained + (long)DescriptorBytes);
            context.Cancellation.ThrowIfCancellationRequested();
            // Own the actual retained prefix before allocation. Encoded input
            // remains borrowed; governed faces never enter the idle-storage pool.
            var ownerPool = context.HasMemoryBudget ? null : pool;
            var owned = ownerPool is null ? GC.AllocateUninitializedArray<byte>(retained)
                : ownerPool.RentPreservingIdle(retained);
            input[..retained].CopyTo(owned);
            var font = new TrueTypeFont(owned, input.Length, context) { storageOwner = context, storagePool = ownerPool };
            return font;
        }
        catch { context.Dispose(); throw; }
    }

    internal static OperationContext InputContext(int length, ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        try
        {
            if (length > context.Limits.MaxInputBytes) throw new ImageLimitException(ImageLimitKind.InputBytes, "Font input limit exceeded.");
            if (length > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "Owned font exceeds the managed array limit.");
            context.ReserveBorrowed(length);
            context.Cancellation.ThrowIfCancellationRequested();
            return context;
        }
        catch { context.Dispose(); throw; }
    }

    // Keep every current consumer, including tables parsed lazily by text work.
    // New TableForText/Find consumers must be added here before use.
    private static bool RetainedTable(uint tag) => tag is
        0x47444546 or 0x47504F53 or 0x47535542 // GDEF, GPOS, GSUB
        or 0x636D6170 or 0x676C7966 or 0x68656164 // cmap, glyf, head
        or 0x68686561 or 0x686D7478 or 0x6B65726E // hhea, hmtx, kern
        or 0x6C6F6361 or 0x6D617870 // loca, maxp
        or 0x76686561 or 0x766D7478; // vhea, vmtx

    private static int RetainedLength(ReadOnlySpan<byte> input, OperationContext context)
    {
        // Classification is not parsing. Malformed directories take the original
        // full-copy parser, retaining its ordered data/unsupported-feature errors.
        // Admission refusal is never retried through this path.
        if (input.Length < 12) return input.Length;
        var version = U32(input);
        if (version != 0x00010000 && version != Tag("true"u8)) return input.Length;
        var count = U16(input[4..]); var directoryBytes = 12 + count * 16;
        if (count == 0 || directoryBytes > input.Length || count > context.Limits.MaxContainerItems)
            return input.Length;
        var retained = directoryBytes; uint previous = 0;
        for (var i = 0; i < count; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var entry = input.Slice(12 + i * 16, 16);
            var tag = U32(entry); var offset = U32(entry[8..]); var length = U32(entry[12..]);
            if (i != 0 && tag <= previous || offset > input.Length || length > input.Length - (long)offset
                || length != 0 && (offset < directoryBytes || (offset & 3) != 0))
                return input.Length;
            if (RetainedTable(tag)) retained = Math.Max(retained, (int)(offset + length));
            previous = tag;
        }
        return retained;
    }

    /// <summary>Map a Unicode scalar to a glyph index, returning zero for an absent character.</summary>
    public int GetGlyphIndex(Rune character, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        using var context = Context(limits, cancellationToken);
        return Map(character.Value, context);
    }

    /// <summary>Read a Unicode scalar's unscaled metrics, using glyph zero when absent.</summary>
    public FontGlyphMetrics GetGlyphMetrics(Rune character, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        using var context = Context(limits, cancellationToken);
        return Metrics(Map(character.Value, context), context);
    }

    /// <summary>Read unscaled metrics by glyph index.</summary>
    public FontGlyphMetrics GetGlyphMetrics(int glyphIndex, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        if ((uint)glyphIndex >= GlyphCount) throw new ArgumentOutOfRangeException(nameof(glyphIndex));
        using var context = Context(limits, cancellationToken);
        return Metrics(glyphIndex, context);
    }

    private OperationContext Context(ImageLimits? limits, CancellationToken cancellationToken)
    {
        EnsureAlive();
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.ReserveBorrowed(StorageBytes); return context;
    }

    internal void EnsureAlive() => ObjectDisposedException.ThrowIf(disposed, this);

    /// <summary>Release owned font storage and its admission. Synchronize with all font users.</summary>
    /// <remarks>Previously prepared layouts own their decoded outlines independently.</remarks>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; var released = data; data = [];
        Interlocked.Exchange(ref storageOwner, null)?.Dispose();
        Interlocked.Exchange(ref storagePool, null)?.ReturnLatest(released);
    }

    // Format facts: Microsoft OpenType specifications otff/head/hhea/maxp/hmtx/loca/glyf.
    // All offsets are range-checked before conversion to managed indexes.
    private TrueTypeFont(byte[] data, int encodedLength, OperationContext context)
    {
        this.data = data;
        if (data.Length < 12) throw new ImageDataException("Truncated font directory.");
        var version = U32(data);
        if (version != 0x00010000 && version != Tag("true"u8))
            throw new UnsupportedImageException(UnsupportedImageFeature.FontContainer, "Expected a standalone TrueType outline font.");
        tableCount = U16(data.AsSpan(4));
        if (tableCount == 0 || 12L + tableCount * 16L > data.Length) throw new ImageDataException("Invalid font table directory.");
        Items(tableCount, context);
        uint previous = 0;
        for (var i = 0; i < tableCount; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var entry = data.AsSpan(12 + i * 16, 16);
            var tag = U32(entry); var offset = U32(entry[8..]); var length = U32(entry[12..]);
            if (i != 0 && tag <= previous || offset > encodedLength || length > encodedLength - (long)offset
                || length != 0 && (offset < 12 + tableCount * 16 || (offset & 3) != 0)
                || (offset > data.Length || length > data.Length - (long)offset) && RetainedTable(tag))
                throw new ImageDataException("Invalid font table range or ordering.");
            previous = tag;
        }
        var head = Table("head"u8, context); var hhea = Table("hhea"u8, context); var maxp = Table("maxp"u8, context);
        if (head.Length < 54 || hhea.Length < 36 || maxp.Length < 32) throw new ImageDataException("Truncated font metrics header.");
        if (U16(head) != 1 || U16(hhea) != 1 || U16(maxp) != 1 || I16(head[52..]) != 0 || I16(hhea[32..]) != 0)
            throw new UnsupportedImageException(UnsupportedImageFeature.FontTable, "Unsupported TrueType metrics or glyph table version.");
        if (U32(head[12..]) != 0x5F0F3CF5) throw new ImageDataException("Invalid TrueType header identifier.");
        UnitsPerEm = U16(head[18..]); GlyphCount = U16(maxp[4..]); metricCount = U16(hhea[34..]);
        Ascender = I16(hhea[4..]); Descender = I16(hhea[6..]); LineGap = I16(hhea[8..]);
        Style = (FontStyle)(U16(head[44..]) & 3);
        if (UnitsPerEm is < 16 or > 16384 || GlyphCount == 0 || metricCount == 0 || metricCount > GlyphCount || Ascender <= Descender)
            throw new ImageDataException("Invalid TrueType units or horizontal metrics.");
        Items(GlyphCount, context);
        var locationFormat = I16(head[50..]);
        if (locationFormat is not (0 or 1)) throw new UnsupportedImageException(UnsupportedImageFeature.FontTable, "Unsupported glyph location format.");
        locationWidth = locationFormat == 0 ? 2 : 4;
        glyf = Required("glyf"u8, context); loca = Required("loca"u8, context); hmtx = Required("hmtx"u8, context);
        if ((GlyphCount + 1L) * locationWidth > loca.Length || metricCount * 4L + (GlyphCount - metricCount) * 2L > hmtx.Length)
            throw new ImageDataException("Truncated glyph locations or horizontal metrics.");
        // Select the validated entry representation once. Typed views do not
        // copy storage; each read retains the encoded big-endian loca value.
        var locations = Span(loca); var glyphs = Span(glyf);
        if (locationWidth == 2)
            ValidateGlyphHeaders(MemoryMarshal.Cast<byte, ushort>(locations), glyphs, GlyphCount, context.Cancellation);
        else
            ValidateGlyphHeaders(MemoryMarshal.Cast<byte, uint>(locations), glyphs, GlyphCount, context.Cancellation);
        cmap = SelectMapping(context); cmapFormat = U16(Span(cmap));
        ValidateMapping(context);
        mappingCount = cmapFormat == 4 ? U16(Span(cmap)[6..]) / 2 : (int)U32(Span(cmap)[12..]);
    }

    private static void ValidateGlyphHeaders<T>(ReadOnlySpan<T> locations, ReadOnlySpan<byte> glyphs,
        int glyphCount, CancellationToken cancellation) where T : unmanaged, IBinaryInteger<T>
    {
        var glyphBytes = glyphs.Length;
        var first = GlyphLocation(locations[0]);
        if (first > glyphBytes) throw new ImageDataException("Invalid glyph location.");
        // A token with no source cannot be canceled. Select that loop once;
        // cancelable tokens retain the original checkpoint before every glyph.
        if (!cancellation.CanBeCanceled)
        {
            if (typeof(T) == typeof(uint) && Avx2.IsSupported
                && TryLongGlyphHeaders(MemoryMarshal.Cast<T, uint>(locations), glyphs, glyphCount, first)) return;
            for (var i = 1; i <= glyphCount; i++)
            {
                if (Avx2.IsSupported && i <= glyphCount - 3
                    && TryFourGlyphBounds(locations, glyphs, i, first, out var fourth))
                {
                    first = fourth; i += 3; continue;
                }
                var last = GlyphLocation(locations[i]);
                if (last < first || last > glyphBytes) throw new ImageDataException("Invalid glyph location ordering.");
                if (last != first) Header(glyphs.Slice((int)first, (int)(last - first)));
                first = last;
            }
            return;
        }
        for (var i = 1; i <= glyphCount; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            var last = GlyphLocation(locations[i]);
            if (last < first || last > glyphBytes) throw new ImageDataException("Invalid glyph location ordering.");
            if (last != first) Header(glyphs.Slice((int)first, (int)(last - first)));
            first = last;
        }
    }

    // Speculative classification is private and stateless. On any failure the
    // caller runs the original one-glyph checks, preserving first-error order.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFourGlyphBounds<T>(ReadOnlySpan<T> locations, ReadOnlySpan<byte> glyphs,
        int index, uint first, out uint last) where T : unmanaged, IBinaryInteger<T>
    {
        last = first;
        if (index > locations.Length - 4) return false;
        var batch = locations.Slice(index, 4);
        var a = GlyphLocation(batch[0]); var b = GlyphLocation(batch[1]);
        var c = GlyphLocation(batch[2]); var d = GlyphLocation(batch[3]);
        if (!TryFourGlyphBoundsAtOffsets(glyphs, first, a, b, c, d, recordsValidated: false)) return false;
        last = d; return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFourGlyphBoundsAtOffsets(ReadOnlySpan<byte> glyphs, uint first,
        uint a, uint b, uint c, uint d, bool recordsValidated)
    {
        ulong x0, x1, x2, x3;
        if (recordsValidated)
        {
            x0 = ProvenHeaderBounds(glyphs, first, a); x1 = ProvenHeaderBounds(glyphs, a, b);
            x2 = ProvenHeaderBounds(glyphs, b, c); x3 = ProvenHeaderBounds(glyphs, c, d);
        }
        else if (!TryHeaderBounds(glyphs, first, a, out x0)
            || !TryHeaderBounds(glyphs, a, b, out x1)
            || !TryHeaderBounds(glyphs, b, c, out x2)
            || !TryHeaderBounds(glyphs, c, d, out x3)) return false;
        // Four little-endian loads contain big-endian signed coordinate words.
        // Swap within each word, then compare xmin/ymin with xmax/ymax.
        var raw = Vector256.Create(x0, x1, x2, x3).AsUInt16();
        var words = Avx2.Shuffle(raw.AsByte(), Vector256.Create(
            (byte)1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14,
            1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14)).AsUInt16();
        var maxima = Avx2.ShiftRightLogical128BitLane(words.AsByte(), 4).AsInt16();
        if ((Avx2.MoveMask(Avx2.CompareGreaterThan(words.AsInt16(), maxima).AsByte()) & 0x0F0F0F0F) != 0)
            return false;
        return true;
    }

    // Only the noncancelable long-loca path enters here. The constructor owns
    // its private input copy, so offsets cannot change between classification
    // and gathering. Check each eight-record extent batch, then feed
    // those decoded offsets to the shared bounds kernel. A failed classification
    // returns to the original ordered validator, preserving first-error order.
    private static bool TryLongGlyphHeaders(ReadOnlySpan<uint> locations, ReadOnlySpan<byte> glyphs,
        int glyphCount, uint first)
    {
        if (glyphCount < 8 || glyphCount >= locations.Length) return false;
        var swap = Vector256.Create((byte)3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12,
            3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12);
        var maximum = Vector256.Create(glyphs.Length); var ten = Vector256.Create(10);
        var i = 1;
        for (; i <= glyphCount - 7; i += 8)
        {
            var previous = Avx2.Shuffle(MemoryMarshal.Cast<uint, Vector256<byte>>(locations.Slice(i - 1, 8))[0], swap).AsInt32();
            var current = Avx2.Shuffle(MemoryMarshal.Cast<uint, Vector256<byte>>(locations.Slice(i, 8))[0], swap).AsInt32();
            // A valid offset fits the positive signed array-length range. Reject
            // unsigned high-bit values before interpreting monotonic differences.
            var invalid = Avx2.Or(Avx2.CompareGreaterThan(Vector256<int>.Zero, current),
                Avx2.CompareGreaterThan(current, maximum));
            invalid = Avx2.Or(invalid, Avx2.CompareGreaterThan(previous, current));
            var tooShort = Avx2.AndNot(Avx2.CompareEqual(previous, current),
                Avx2.CompareGreaterThan(ten, Avx2.Subtract(current, previous)));
            if (Avx2.MoveMask(Avx2.Or(invalid, tooShort).AsByte()) != 0) return false;
            var lower = current.GetLower();
            var a = (uint)lower.GetElement(0); var b = (uint)lower.GetElement(1);
            var c = (uint)lower.GetElement(2); var d = (uint)lower.GetElement(3);
            if (!TryFourGlyphBoundsAtOffsets(glyphs, first, a, b, c, d, recordsValidated: true)) return false;
            // Retain the classified upper half in SIMD until the first four
            // headers have been consumed; avoid eight simultaneously live offsets.
            var upper = current.GetUpper();
            var e = (uint)upper.GetElement(0); var f = (uint)upper.GetElement(1);
            var g = (uint)upper.GetElement(2); var h = (uint)upper.GetElement(3);
            if (!TryFourGlyphBoundsAtOffsets(glyphs, d, e, f, g, h, recordsValidated: true)) return false;
            first = h;
        }
        for (; i <= glyphCount - 3; i += 4)
        {
            if (!TryFourGlyphBounds(locations, glyphs, i, first, out var last)) return false;
            first = last;
        }
        // Earlier headers are valid. Remaining locations retain the original
        // checks before a scalar header can report its first error directly.
        for (; i <= glyphCount; i++)
        {
            var last = GlyphLocation(locations[i]);
            if (last < first || last > glyphs.Length) return false;
            if (last != first) Header(glyphs.Slice((int)first, (int)(last - first)));
            first = last;
        }
        return true;
    }

    // These gathers only test whether the contour word is zero. Byte order and
    // sign cannot change that predicate; keep coordinate decoding separate.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ProvenHeaderBounds(ReadOnlySpan<byte> glyphs, uint first, uint last)
    {
        if (first == last) return 0;
        // The structural pass proves this record has at least ten bytes. Keep
        // the ordinary bounded span slice; no unchecked reference or widened read.
        var header = glyphs.Slice((int)first, 10);
        return MemoryMarshal.Read<ushort>(header) == 0 ? 0 : BinaryPrimitives.ReadUInt64LittleEndian(header.Slice(2, 8));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryHeaderBounds(ReadOnlySpan<byte> glyphs, uint first, uint last, out ulong bounds)
    {
        bounds = 0;
        if (last < first || last > glyphs.Length) return false;
        if (last == first) return true;
        if (last - first < 10) return false;
        var header = glyphs.Slice((int)first, 10);
        if (MemoryMarshal.Read<ushort>(header) != 0) bounds = BinaryPrimitives.ReadUInt64LittleEndian(header.Slice(2, 8));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GlyphLocation<T>(T value) where T : unmanaged, IBinaryInteger<T>
    {
        // These private instantiations are ushort/uint only. Type and host byte
        // order tests fold when each value-type scan is compiled.
        if (typeof(T) == typeof(ushort))
        {
            var bits = ushort.CreateTruncating(value);
            return (uint)(BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(bits) : bits) * 2;
        }
        var location = uint.CreateTruncating(value);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(location) : location;
    }

    private FontGlyphMetrics Metrics(int glyphIndex, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested(); var metrics = Span(hmtx);
        var advance = U16(metrics[(Math.Min(glyphIndex, metricCount - 1) * 4)..]);
        var bearing = I16(metrics[(glyphIndex < metricCount ? glyphIndex * 4 + 2 : metricCount * 4 + (glyphIndex - metricCount) * 2)..]);
        var glyph = Glyph(glyphIndex);
        return glyph.Length == 0 || I16(glyph) == 0
            ? new(glyphIndex, advance, bearing, 0, 0, 0, 0, false)
            : new(glyphIndex, advance, bearing, I16(glyph[2..]), I16(glyph[4..]), I16(glyph[6..]), I16(glyph[8..]), true);
    }

    private ReadOnlySpan<byte> Glyph(int glyphIndex)
    {
        var first = Location(glyphIndex); var last = Location(glyphIndex + 1);
        return data.AsSpan(glyf.Offset + (int)first, (int)(last - first));
    }
    private uint Location(int index) => locationWidth == 2 ? (uint)U16(Span(loca)[(index * 2)..]) * 2 : U32(Span(loca)[(index * 4)..]);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Header(ReadOnlySpan<byte> glyph)
    {
        if (glyph.Length < 10) throw new ImageDataException("Invalid glyph header.");
        if (I16(glyph) != 0)
        {
            // All four signed coordinates occupy this validated eight-byte extent.
            var bounds = BinaryPrimitives.ReadUInt64BigEndian(glyph.Slice(2, 8));
            bool reversed;
            if (Sse2.IsSupported)
            {
                // Low signed words are ymax,xmax,ymin,xmin after the same
                // big-endian load. Compare ymin/xmin with ymax/xmax together;
                // the upper comparison lanes are outside the two coordinates.
                var packed = Vector128.CreateScalar(bounds).AsInt16();
                var minima = Sse2.ShiftRightLogical128BitLane(packed.AsByte(), 4).AsInt16();
                reversed = (Sse2.MoveMask(Sse2.CompareGreaterThan(minima, packed).AsByte()) & 15) != 0;
            }
            else reversed = unchecked((short)(bounds >> 48)) > unchecked((short)(bounds >> 16))
                || unchecked((short)(bounds >> 32)) > unchecked((short)bounds);
            if (reversed)
                throw new ImageDataException("Invalid glyph bounds.");
        }
    }

    private ReadOnlySpan<byte> Table(ReadOnlySpan<byte> tag, OperationContext context) => Span(Required(tag, context));
    private TableRange Required(ReadOnlySpan<byte> tag, OperationContext context)
        => Find(tag, context) ?? throw new ImageDataException("Required TrueType table is missing.");
    private TableRange? Find(ReadOnlySpan<byte> tag, OperationContext context)
    {
        var target = Tag(tag); var low = 0; var high = tableCount - 1;
        while (low <= high)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var mid = (low + high) / 2; var entry = data.AsSpan(12 + mid * 16, 16); var actual = U32(entry);
            if (target < actual) high = mid - 1;
            else if (target > actual) low = mid + 1;
            else return new((int)U32(entry[8..]), (int)U32(entry[12..]));
        }
        return null;
    }
    private ReadOnlySpan<byte> Span(TableRange range) => data.AsSpan(range.Offset, range.Length);
    private static void Items(long count, OperationContext context)
    {
        if (count > context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Font item limit exceeded.");
    }
    private static uint Tag(ReadOnlySpan<byte> tag) => U32(tag);
    private static ushort U16(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadUInt16BigEndian(bytes);
    private static short I16(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadInt16BigEndian(bytes);
    private static uint U32(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadUInt32BigEndian(bytes);
}
