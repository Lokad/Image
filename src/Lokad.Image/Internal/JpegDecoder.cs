using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Owned implementation of T.81 Annexes B, F and G for 8-bit Huffman JPEG.</summary>
internal ref partial struct JpegDecoder
{
    private readonly ReadOnlySpan<byte> data;
    private readonly OperationContext context;
    private readonly ImageInfo info;
    private readonly ushort[]?[] quantization;
    private readonly byte[] quantizationPrecision;
    private readonly JpegHuffman?[] dcTables;
    private readonly JpegHuffman?[] acTables;
    private Component[] components;
    private int position, maxH, maxV, mcuColumns, mcuRows, restartInterval;
    private bool progressive, frameSeen, scanSeen, rgb;

    private sealed class Component
    {
        public int Id, H, V, Quantization, BlockColumns, BlockRows, SampleWidth, SampleHeight;
        public int Predictor;
        public int[] Coefficients = [];
        public int[] Approximation = [];
        public byte[] Plane = [];
        public ushort[]? FrozenQuantization;
    }

    public JpegDecoder(ReadOnlySpan<byte> data, OperationContext context,
        ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
    {
        this.data = data; this.context = context;
        info = ContainerHeaders.Jpeg(data, context, metadataPolicy);
        context.Reserve(1024); // Fixed parser references and component descriptors.
        quantization = new ushort[4][]; quantizationPrecision = new byte[4];
        dcTables = new JpegHuffman[4]; acTables = new JpegHuffman[4];
        components = []; position = 2; maxH = 0; maxV = 0; mcuColumns = 0; mcuRows = 0; restartInterval = 0;
        progressive = false; frameSeen = false; scanSeen = false; rgb = false;
    }

    public ImageBuffer Decode() => DecodeCore(validateOnly: false)!;

    internal ImageInfo ValidatePixels()
    {
        DecodeCore(validateOnly: true);
        return info;
    }

    private ImageBuffer? DecodeCore(bool validateOnly)
    {
        var adobeTransform = -1;
        try
        {
            while (position < data.Length)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var marker = ReadMarker();
                if (marker == 0xD9)
                {
                    if (!scanSeen) throw new ImageDataException("JPEG has no pixel scans.");
                    foreach (var component in components)
                        if (component.Approximation[0] < 0) throw new ImageDataException("JPEG component is missing a DC scan.");
                    if (validateOnly)
                    {
                        // Sampling is checked in ReadFrame; entropy/table/progression
                        // validity is complete. Rendering adds only the color-transform
                        // support check, then total fixed-point arithmetic and placement.
                        _ = ResolveSampleColorSpace(JpegColorSpace.Auto, adobeTransform);
                        context.Cancellation.ThrowIfCancellationRequested();
                        return null;
                    }
                    if (components.Length == 4) return RenderCmyk(adobeTransform);
                    if (adobeTransform == 2) throw UnsupportedColorTransform();
                    return Render();
                }
                var payload = ReadSegment();
                switch (marker)
                {
                    case 0xC0 or 0xC2: ReadFrame(payload, marker == 0xC2); break;
                    case 0xDB: ReadQuantization(payload); break;
                    case 0xC4: ReadHuffman(payload); break;
                    case 0xDD:
                        if (payload.Length != 2) throw new ImageDataException("Invalid JPEG restart interval.");
                        restartInterval = BinaryPrimitives.ReadUInt16BigEndian(payload); break;
                    case 0xDA: ReadScan(payload); scanSeen = true; break;
                    case 0xEE when payload.StartsWith("Adobe"u8):
                        if (payload.Length < 12) throw new ImageDataException("Truncated Adobe JPEG marker.");
                        if (payload[11] > 2) throw UnsupportedColorTransform();
                        adobeTransform = payload[11];
                        rgb = payload[11] == 0; break;
                    case >= 0xE0 and <= 0xEF or 0xFE: break;
                    default: throw new UnsupportedImageException(UnsupportedImageFeature.ControlMarker, "Unsupported JPEG control marker.", ImageFormat.Jpeg);
                }
            }
            throw new ImageDataException("JPEG is missing its end marker.");
        }
        finally
        {
            foreach(var component in components)
            {
                if(component is null || component.Coefficients.Length==0)continue;
                var storage=component.Coefficients;component.Coefficients=[];
                context.ReturnCoefficients(storage);
            }
        }
    }

    private int ReadMarker()
    {
        if (position >= data.Length || data[position++] != 255) throw new ImageDataException("Invalid JPEG marker.");
        while (position < data.Length && data[position] == 255) { context.Cancellation.ThrowIfCancellationRequested(); position++; }
        if (position >= data.Length) throw new ImageDataException("Truncated JPEG marker.");
        var marker = data[position++];
        if (marker is 0 or 0xD8 or >= 0xD0 and <= 0xD7) throw new ImageDataException("Unexpected JPEG marker.");
        return marker;
    }

    private ReadOnlySpan<byte> ReadSegment()
    {
        if (position > data.Length - 2) throw new ImageDataException("Truncated JPEG segment.");
        var length = BinaryPrimitives.ReadUInt16BigEndian(data[position..]);
        if (length < 2 || length > data.Length - position) throw new ImageDataException("Invalid JPEG segment length.");
        var payload = data.Slice(position + 2, length - 2); position += length;
        context.Cancellation.ThrowIfCancellationRequested();
        return payload;
    }

    private void ReadFrame(ReadOnlySpan<byte> payload, bool isProgressive)
    {
        if (frameSeen || payload.Length < 6) throw new ImageDataException("Invalid JPEG frame.");
        progressive = isProgressive; frameSeen = true;
        var count = payload[5];
        if (count is not (1 or 3 or 4) || payload.Length != 6 + count * 3) throw new ImageDataException("Invalid JPEG components.");
        components = new Component[count];
        int totalBlocks = 0;
        for (var i = 0; i < count; i++)
        {
            var offset = 6 + i * 3;
            var h = payload[offset + 1] >> 4; var v = payload[offset + 1] & 15; var q = payload[offset + 2];
            if (h is < 1 or > 4 || v is < 1 or > 4 || q > 3) throw new ImageDataException("Invalid JPEG sampling factors.");
            for (var previous = 0; previous < i; previous++)
                if (components[previous].Id == payload[offset]) throw new ImageDataException("Duplicate JPEG component identifier.");
            components[i] = new Component { Id = payload[offset], H = h, V = v, Quantization = q };
            maxH = Math.Max(maxH, h); maxV = Math.Max(maxV, v); totalBlocks += h * v;
        }
        if (totalBlocks > 10) throw new ImageDataException("JPEG MCU contains too many blocks.");
        if (count == 1)
        {
            // In a noninterleaved grayscale scan each MCU is one block; sampling
            // factors do not change the single component's raster dimensions.
            components[0].H = 1; components[0].V = 1; maxH = 1; maxV = 1;
        }
        else if (count == 3 && (components[0].H != maxH || components[0].V != maxV
            || components[1].H != components[2].H || components[1].V != components[2].V
            || maxH % components[1].H != 0 || maxV % components[1].V != 0
            || maxH / components[1].H is not (1 or 2 or 4)
            || maxV / components[1].V is not (1 or 2)
            || (maxH == components[1].H && maxV != components[1].V)))
            throw new UnsupportedImageException(UnsupportedImageFeature.SamplingLayout, "Unsupported JPEG component sampling layout.", ImageFormat.Jpeg);
        if (count == 3 && components[0].Id == 'R' && components[1].Id == 'G' && components[2].Id == 'B') rgb = true;
        mcuColumns = (info.Width + 8 * maxH - 1) / (8 * maxH);
        mcuRows = (info.Height + 8 * maxV - 1) / (8 * maxV);
        foreach (var component in components)
        {
            component.BlockColumns = mcuColumns * component.H; component.BlockRows = mcuRows * component.V;
            component.SampleWidth = (info.Width * component.H + maxH - 1) / maxH;
            component.SampleHeight = (info.Height * component.V + maxV - 1) / maxV;
            var countValues = (long)component.BlockColumns * component.BlockRows * 64;
            if (countValues > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "JPEG coefficient plane is too large.");
            context.Reserve(countValues * 4 + 64 * 4);
            component.Coefficients = context.RentCoefficients((int)countValues);
            component.Approximation = new int[64]; Array.Fill(component.Approximation, -1);
        }
    }

    private void ReadQuantization(ReadOnlySpan<byte> payload)
    {
        while (!payload.IsEmpty)
        {
            var precision = payload[0] >> 4; var id = payload[0] & 15;
            var length = 1 + 64 * (precision + 1);
            if (precision > 1 || id > 3 || payload.Length < length) throw new ImageDataException("Invalid JPEG quantization table.");
            context.Reserve(128);
            var values = new ushort[64];
            for (var i = 0; i < 64; i++)
            {
                var value = precision == 0 ? payload[i + 1] : BinaryPrimitives.ReadUInt16BigEndian(payload[(i * 2 + 1)..]);
                if (value == 0) throw new ImageDataException("JPEG quantization values must be positive.");
                values[JpegTransform.Zigzag[i]] = (ushort)value;
            }
            if (quantization[id] is not null) context.Release(128);
            quantization[id] = values; quantizationPrecision[id] = (byte)precision; payload = payload[length..];
        }
    }

    private void ReadHuffman(ReadOnlySpan<byte> payload)
    {
        while (!payload.IsEmpty)
        {
            if (payload.Length < 17) throw new ImageDataException("Truncated JPEG Huffman table.");
            var kind = payload[0] >> 4; var id = payload[0] & 15;
            if (kind > 1 || id > 3) throw new ImageDataException("Invalid JPEG Huffman selector.");
            var counts = payload.Slice(1, 16); var symbols = 0;
            foreach (var count in counts) symbols += count;
            if (symbols > payload.Length - 17) throw new ImageDataException("Truncated JPEG Huffman symbols.");
            var table = new JpegHuffman(counts, payload.Slice(17, symbols), context);
            // Redefined table allocations stay charged: the old object may remain
            // live until collection, and hostile repeated definitions remain bounded.
            if (kind == 0) dcTables[id] = table; else acTables[id] = table;
            payload = payload[(17 + symbols)..];
        }
    }

    private void ReadScan(ReadOnlySpan<byte> payload)
    {
        if (!frameSeen || payload.Length < 6 || payload[0] is < 1 or > 4 || payload.Length != 4 + payload[0] * 2)
            throw new ImageDataException("Invalid JPEG scan selectors.");
        var count = payload[0];
        Span<int> indexes = stackalloc int[4]; Span<int> dc = stackalloc int[4]; Span<int> ac = stackalloc int[4];
        var ss = payload[1 + 2 * count]; var se = payload[2 + 2 * count];
        var ah = payload[3 + 2 * count] >> 4; var al = payload[3 + 2 * count] & 15;
        if (!progressive && (ss != 0 || se != 63 || ah != 0 || al != 0)) throw new ImageDataException("Invalid baseline JPEG scan parameters.");
        if (progressive && (ss > se || se > 63 || (ss == 0 && se != 0) || (ss != 0 && count != 1)
            || ah > 13 || al > 13 || (ah != 0 && ah != al + 1)))
            throw new ImageDataException("Invalid progressive JPEG scan parameters.");
        for (var i = 0; i < count; i++)
        {
            var id = payload[1 + i * 2]; indexes[i] = -1;
            for (var j = 0; j < components.Length; j++) if (components[j].Id == id) indexes[i] = j;
            if (indexes[i] < 0) throw new ImageDataException("Unknown JPEG scan component.");
            for (var j = 0; j < i; j++) if (indexes[j] == indexes[i]) throw new ImageDataException("Duplicate JPEG scan component.");
            dc[i] = payload[2 + i * 2] >> 4; ac[i] = payload[2 + i * 2] & 15;
            if (dc[i] > 3 || ac[i] > 3) throw new ImageDataException("Invalid JPEG scan table selector.");
            if (!progressive && (dc[i] > 1 || ac[i] > 1)) throw new ImageDataException("Baseline JPEG Huffman selectors must be zero or one.");
            var component = components[indexes[i]];
            if (progressive && ss != 0 && component.Approximation[0] < 0) throw new ImageDataException("JPEG AC scan precedes DC.");
            for (var k = ss; k <= se; k++)
            {
                if ((ah == 0 && component.Approximation[k] != -1) || (ah != 0 && component.Approximation[k] != ah))
                    throw new ImageDataException("Invalid JPEG scan progression.");
                component.Approximation[k] = al;
            }
            if (component.FrozenQuantization is null)
            {
                var table = quantization[component.Quantization] ?? throw new ImageDataException("Missing JPEG quantization table.");
                if (!progressive && quantizationPrecision[component.Quantization] != 0)
                    throw new ImageDataException("Baseline JPEG requires 8-bit quantization tables.");
                context.Reserve(128); component.FrozenQuantization = table.ToArray();
            }
            component.Predictor = 0;
            if (ss == 0 && ah == 0 && dcTables[dc[i]] is null) throw new ImageDataException("Missing JPEG DC Huffman table.");
            if (se != 0 && acTables[ac[i]] is null) throw new ImageDataException("Missing JPEG AC Huffman table.");
        }
        var interleaved = count > 1;
        var first = components[indexes[0]];
        var columns = interleaved ? mcuColumns : (first.SampleWidth + 7) / 8;
        var rows = interleaved ? mcuRows : (first.SampleHeight + 7) / 8;
        var reader = new JpegEntropyReader(data, position, context);
        int eobRun = 0, expectedRestart = 0;
        for (var mcu = 0; mcu < columns * rows; mcu++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (restartInterval != 0 && mcu != 0 && mcu % restartInterval == 0)
            {
                if (eobRun != 0) throw new ImageDataException("JPEG EOB run crosses a restart interval.");
                reader.Restart(expectedRestart); expectedRestart = (expectedRestart + 1) & 7;
                for (var i = 0; i < count; i++) components[indexes[i]].Predictor = 0;
            }
            for (var i = 0; i < count; i++)
            {
                var component = components[indexes[i]];
                var horizontal = interleaved ? component.H : 1; var vertical = interleaved ? component.V : 1;
                for (var by = 0; by < vertical; by++)
                for (var bx = 0; bx < horizontal; bx++)
                {
                    var x = (mcu % columns) * horizontal + bx; var y = (mcu / columns) * vertical + by;
                    var block = component.Coefficients.AsSpan((y * component.BlockColumns + x) * 64, 64);
                    if (!progressive)
                        ReadBaselineBlock(ref reader, block, ref component.Predictor, dcTables[dc[i]]!, acTables[ac[i]]!, context);
                    else
                        ReadBlock(ref reader, block, component, dcTables[dc[i]], acTables[ac[i]], ss, se, ah, al, ref eobRun);
                }
            }
        }
        if (eobRun != 0) throw new ImageDataException("JPEG EOB run exceeds the scan.");
        reader.Align(); position = reader.Position;
    }

    internal static void ReadBaselineBlock(ref JpegEntropyReader reader, Span<int> block, ref int predictor,
        JpegHuffman dc, JpegHuffman ac, OperationContext context)
    {
        if (block.Length != 64) throw new ArgumentException("JPEG coefficient blocks have 64 entries.", nameof(block));
        // T.81 F.2.2: ReadScan has already admitted baseline SS=0, SE=63,
        // Ah=Al=0. Keep symbol/amplitude traversal and every failure boundary
        // while removing progressive/refinement state from this hot loop.
        context.Cancellation.ThrowIfCancellationRequested();
        {
            var category = dc.Read(ref reader);
            if (category > 11) throw new ImageDataException("Invalid JPEG DC coefficient category.");
            var nextPredictor = (long)predictor + reader.ReadSigned(category);
            if (nextPredictor is < -32768 or > 32767) throw new ImageDataException("JPEG DC predictor is out of range.");
            predictor = (int)nextPredictor; block[0] = predictor;
            if (context.Cancellation.CanBeCanceled)
            {
                ReadCancelableBaselineAc(ref reader, block, ac);
                return;
            }
            var k = 1;
            while (k <= 63)
            {
                ac.ReadPlainBaselineAc(ref reader, block, ref k);
                if (k > 63) return;
                if (ac.TryReadBaselineAc(ref reader, 64 - k, out var fastSymbol, out var amplitude))
                {
                    k += fastSymbol >> 4;
                    block[JpegTransform.Zigzag[k++]] = amplitude;
                    continue;
                }
                var symbol = ac.Read(ref reader); var run = symbol >> 4; var size = symbol & 15;
                if (size == 0)
                {
                    if (run == 15) { k += 16; if (k > 64) throw new ImageDataException("JPEG zero run exceeds band."); continue; }
                    if (run != 0) throw new ImageDataException("Invalid baseline JPEG end-of-block symbol.");
                    return;
                }
                if (size > 10) throw new ImageDataException("Invalid JPEG AC coefficient category.");
                k += run;
                if (k > 63) throw new ImageDataException("JPEG coefficient run exceeds band.");
                block[JpegTransform.Zigzag[k++]] = reader.ReadSigned(size);
            }
        }
    }

    private static void ReadCancelableBaselineAc(ref JpegEntropyReader reader, Span<int> block, JpegHuffman ac)
    {
        var k = 1;
        while (k <= 63)
        {
            ac.ReadPlainCancelableAc(ref reader, block, ref k);
            if (k > 63) return;
            var symbol = ac.Read(ref reader); var run = symbol >> 4; var size = symbol & 15;
            if (size == 0)
            {
                if (run == 15) { k += 16; if (k > 64) throw new ImageDataException("JPEG zero run exceeds band."); continue; }
                if (run != 0) throw new ImageDataException("Invalid baseline JPEG end-of-block symbol.");
                return;
            }
            if (size > 10) throw new ImageDataException("Invalid JPEG AC coefficient category.");
            k += run;
            if (k > 63) throw new ImageDataException("JPEG coefficient run exceeds band.");
            block[JpegTransform.Zigzag[k++]] = reader.ReadSigned(size);
        }
    }

    private void ReadBlock(ref JpegEntropyReader reader, Span<int> block, Component component,
        JpegHuffman? dc, JpegHuffman? ac, int ss, int se, int ah, int al, ref int eobRun)
    {
        // Progressive scans revisit spectral bands. Retain their coefficients
        // in scan order and map to natural order once, after the final scan.
        context.Cancellation.ThrowIfCancellationRequested();
        if (ss == 0)
        {
            if (ah == 0)
            {
                var size = dc!.Read(ref reader);
                if (size > 11) throw new ImageDataException("Invalid JPEG DC coefficient category.");
                var predictor = (long)component.Predictor + reader.ReadSigned(size);
                if (predictor is < -32768 or > 32767) throw new ImageDataException("JPEG DC predictor is out of range.");
                component.Predictor = (int)predictor; block[0] = component.Predictor << al;
            }
            else block[0] |= reader.ReadBits(1) << al;
            if (se == 0) return;
            ss = 1;
        }
        if (!progressive || ah == 0)
        {
            if (eobRun > 0) { eobRun--; return; }
            var k = ss;
            while (k <= se)
            {
                var symbol = ac!.Read(ref reader); var run = symbol >> 4; var size = symbol & 15;
                if (size == 0)
                {
                    if (run == 15) { k += 16; if (k > se + 1) throw new ImageDataException("JPEG zero run exceeds band."); continue; }
                    if (!progressive && run != 0) throw new ImageDataException("Invalid baseline JPEG end-of-block symbol.");
                    eobRun = progressive ? (1 << run) + reader.ReadBits(run) - 1 : 0; return;
                }
                if (size > 10) throw new ImageDataException("Invalid JPEG AC coefficient category.");
                k += run;
                if (k > se) throw new ImageDataException("JPEG coefficient run exceeds band.");
                block[k++] = reader.ReadSigned(size) << al;
            }
            return;
        }

        // Successive-approximation AC decoding: existing nonzero coefficients
        // consume correction bits while run counts count only zero coefficients.
        if (ac!.TryReadRefinementBlock(ref reader, block, ss, se, al, ref eobRun)) return;
        if (reader.HasCancelableToken && ac.TryReadCancelableRefinementBlock(ref reader, block, ss, se, al, ref eobRun)) return;
        if (Avx2.IsSupported && Bmi2.X64.IsSupported)
        {
            ReadMaskedRefinement(ref reader, block, ac!, ss, se, al, ref eobRun);
            return;
        }
        var positionInBand = ss;
        var increment = 1 << al;
        if (eobRun == 0)
        {
            while (positionInBand <= se)
            {
                var symbol = ac!.Read(ref reader); var run = symbol >> 4; var size = symbol & 15;
                var newValue = 0;
                if (size != 0)
                {
                    if (size != 1) throw new ImageDataException("Invalid JPEG refinement symbol.");
                    newValue = reader.ReadBits(1) == 0 ? -increment : increment;
                }
                else if (run != 15)
                {
                    eobRun = (1 << run) + reader.ReadBits(run); break;
                }
                else run = 16;
                while (positionInBand <= se)
                {
                    if (TryRefineRun(ref reader, block, ref positionInBand, se, ref run, newValue != 0, increment, out var finished))
                    {
                        if (finished) break;
                        continue;
                    }
                    ref var value = ref block[positionInBand];
                    if (value != 0) Refine(ref reader, ref value, increment);
                    else
                    {
                        if (run == 0) break;
                        run--;
                        if (run == 0 && newValue == 0) { positionInBand++; break; }
                    }
                    positionInBand++;
                }
                if (run != 0 || (newValue != 0 && positionInBand > se)) throw new ImageDataException("JPEG refinement run exceeds band.");
                if (newValue != 0) block[positionInBand++] = newValue;
            }
        }
        if (eobRun > 0)
        {
            var tailZeros = int.MaxValue;
            while (positionInBand <= se)
            {
                if (TryRefineRun(ref reader, block, ref positionInBand, se, ref tailZeros, false, increment, out _)) continue;
                ref var value = ref block[positionInBand++];
                if (value != 0) Refine(ref reader, ref value, increment);
            }
            eobRun--;
        }
    }

    private static void Refine(ref JpegEntropyReader reader, ref int value, int increment)
    {
        if (reader.ReadBits(1) != 0 && (Math.Abs(value) & increment) == 0)
            value += value > 0 ? increment : -increment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ApplyCorrection(ref int value, int increment)
    {
        // T.81 G.1.2 updates signed magnitudes. Integer masks retain both the
        // already-refined bit check and the original treatment of zero, while
        // avoiding data-dependent branches on the coefficient's sign.
        var sign = value >> 31;
        var magnitude = (value ^ sign) - sign;
        if (magnitude < 0) _ = Math.Abs(value); // preserve Int32.MinValue's failure
        if ((magnitude & increment) == 0)
        {
            var nonPositive = (value - 1) >> 31;
            value += (increment ^ nonPositive) - nonPositive;
        }
    }

    private static void ReadMaskedRefinement(ref JpegEntropyReader reader, Span<int> block,
        JpegHuffman ac, int start, int end, int low, ref int eobRun)
    {
        // T.81 G.1.2: corrections preserve nonzero status. Discover positions
        // once; subsequent symbols only add a newly inserted coefficient.
        ulong nonzero = 0;
        for (var i = 0; i < 64; i += 8)
        {
            var values = Vector256.Create(block.Slice(i, 8));
            var zeros = (uint)Avx.MoveMask(Avx2.CompareEqual(values, Vector256<int>.Zero).AsSingle());
            nonzero |= (ulong)(~zeros & 255) << i;
        }
        var bandEnd = end == 63 ? ulong.MaxValue : (1UL << (end + 1)) - 1;
        var position = start; var increment = 1 << low;
        if (eobRun == 0)
        {
            while (position <= end)
            {
                var symbol = ac.Read(ref reader); var run = symbol >> 4; var size = symbol & 15;
                var newValue = 0;
                if (size != 0)
                {
                    if (size != 1) throw new ImageDataException("Invalid JPEG refinement symbol.");
                    newValue = reader.ReadBits(1) == 0 ? -increment : increment;
                }
                else if (run != 15) { eobRun = (1 << run) + reader.ReadBits(run); break; }
                else run = 16;
                var remainingBand = bandEnd & (ulong.MaxValue << position);
                var zeros = ~nonzero & remainingBand;
                var target = Bmi2.X64.ParallelBitDeposit(1UL << (newValue != 0 ? run : run - 1), zeros);
                var stop = target == 0 ? end + 1 : BitOperations.TrailingZeroCount(target) + (newValue != 0 ? 0 : 1);
                var prefix = (stop == 64 ? ulong.MaxValue : (1UL << stop) - 1) & remainingBand;
                // Refine before reporting an overlong zero run, retaining
                // the original entropy/format failure precedence.
                RefineMaskedBits(ref reader, block, nonzero & prefix, increment);
                run -= BitOperations.PopCount(zeros & prefix); position = stop;
                if (run != 0 || (newValue != 0 && position > end)) throw new ImageDataException("JPEG refinement run exceeds band.");
                if (newValue != 0)
                {
                    block[position] = newValue; nonzero |= 1UL << position; position++;
                }
            }
        }
        if (eobRun > 0)
        {
            if (position <= end) RefineMaskedBits(ref reader, block, nonzero & bandEnd & (ulong.MaxValue << position), increment);
            eobRun--;
        }
    }

    internal static void RefineMaskedBits(ref JpegEntropyReader reader, Span<int> block, ulong positions, int increment)
    {
        // A cancelable reader always refuses grouped correction reads. Select
        // the original bit loop once instead of repeating batch setup per bit.
        if (!Bmi2.X64.IsSupported || reader.HasCancelableToken)
        {
            while (positions != 0)
            {
                ref var value = ref block[BitOperations.TrailingZeroCount(positions)];
                Refine(ref reader, ref value, increment); positions &= positions - 1;
            }
            return;
        }
        while (positions != 0)
        {
            var count = Math.Min(16, BitOperations.PopCount(positions));
            var selected = Bmi2.X64.IsSupported ? Bmi2.X64.ParallelBitDeposit((1UL << count) - 1, positions) : 0;
            if (selected != 0 && reader.TryReadCorrectionBits(count, out var bits))
            {
                // Reverse the bounded MSB-first field into increasing positions.
                var reversed = (uint)bits << (16 - count);
                reversed = ((reversed & 0x5555u) << 1) | ((reversed >> 1) & 0x5555u);
                reversed = ((reversed & 0x3333u) << 2) | ((reversed >> 2) & 0x3333u);
                reversed = ((reversed & 0x0F0Fu) << 4) | ((reversed >> 4) & 0x0F0Fu);
                reversed = ((reversed & 255u) << 8) | (reversed >> 8);
                var ones = Bmi2.X64.ParallelBitDeposit(reversed, selected);
                while (ones != 0)
                {
                    ref var value = ref block[BitOperations.TrailingZeroCount(ones)];
                    if ((Math.Abs(value) & increment) == 0) value += value > 0 ? increment : -increment;
                    ones &= ones - 1;
                }
                positions &= ~selected;
            }
            else
            {
                ref var value = ref block[BitOperations.TrailingZeroCount(positions)];
                Refine(ref reader, ref value, increment); positions &= positions - 1;
            }
        }
    }

    internal static bool TryRefineRun(ref JpegEntropyReader reader, Span<int> block, ref int position,
        int end, ref int run, bool inserting, int increment, out bool finished)
    {
        // T.81 G.1.2: a run counts only zeros; each existing nonzero consumes
        // one correction. Bound the group before a new coefficient's zero, or
        // through a ZRL's final zero. EOB tails use an unexhausted zero budget.
        finished = false;
        if (!Avx2.IsSupported || !Bmi2.IsSupported || position > block.Length - 8) return false;
        var values = Vector256.Create(block.Slice(position, 8));
        var count = Math.Min(8, end - position + 1);
        var zeros = (uint)Avx.MoveMask(Avx2.CompareEqual(values, Vector256<int>.Zero).AsSingle()) & ((1u << count) - 1);
        var stop = inserting ? run + 1 : run;
        var zeroCount = BitOperations.PopCount(zeros);
        if (stop <= zeroCount)
        {
            var boundary = zeros;
            for (var i = 1; i < stop; i++) boundary &= boundary - 1;
            count = BitOperations.TrailingZeroCount(boundary) + (inserting ? 0 : 1);
            zeros &= (1u << count) - 1;
            zeroCount = BitOperations.PopCount(zeros);
            finished = true;
        }
        var corrections = count - zeroCount;
        if (corrections != 0)
        {
            // A refused group changes neither reader nor coefficients. The
            // caller then retains its original per-bit failures/cancellation.
            if (!reader.TryReadCorrectionBits(corrections, out var bits)) { finished = false; return false; }
            // First spectral coefficient consumes the oldest (MSB) correction.
            // Reverse the eight-bit field, then deposit it only at nonzero lanes.
            var reversed = (uint)bits << (8 - corrections);
            reversed = ((reversed & 0x55u) << 1) | ((reversed >> 1) & 0x55u);
            reversed = ((reversed & 0x33u) << 2) | ((reversed >> 2) & 0x33u);
            reversed = ((reversed & 0x0Fu) << 4) | ((reversed >> 4) & 0x0Fu);
            var packed = Bmi2.ParallelBitDeposit(reversed, ~zeros & ((1u << count) - 1));
            var lanes = Vector256.Create(1, 2, 4, 8, 16, 32, 64, 128);
            var correctionMask = Avx2.CompareEqual(Avx2.And(Vector256.Create((int)packed), lanes), lanes);
            var missingBit = Avx2.CompareEqual(Avx2.And(Avx2.Abs(values).AsInt32(), Vector256.Create(increment)), Vector256<int>.Zero);
            var delta = Avx2.And(Avx2.Sign(Vector256.Create(increment), values), Avx2.And(correctionMask, missingBit));
            // Lanes beyond the admitted prefix have zero delta, including
            // coefficients outside a short spectral band.
            Avx2.Add(values, delta).CopyTo(block.Slice(position, 8));
        }
        position += count; run -= zeroCount;
        return true;
    }

    private ImageBuffer Render()
    {
        Span<byte> block = stackalloc byte[64];
        foreach (var component in components)
        {
            var length = component.Coefficients.Length;
            context.Reserve(length); component.Plane = new byte[length];
            for (var y = 0; y < component.BlockRows; y++)
            for (var x = 0; x < component.BlockColumns; x++)
            {
                ReadOnlySpan<int> coefficients = component.Coefficients.AsSpan((y * component.BlockColumns + x) * 64, 64);
                if (progressive)
                {
                    // EOI has validated every component's DC progression. The
                    // already admitted progression array is no longer needed;
                    // reuse it for this block without new scratch admission.
                    var natural = component.Approximation.AsSpan();
                    for (var k = 0; k < 64; k++) natural[JpegTransform.Zigzag[k]] = coefficients[k];
                    coefficients = natural;
                }
                JpegInverseTransform.Inverse(coefficients, component.FrozenQuantization!, block, context);
                for (var by = 0; by < 8; by++)
                    block.Slice(by * 8, 8).CopyTo(component.Plane.AsSpan((y * 8 + by) * component.BlockColumns * 8 + x * 8, 8));
            }
            var storage=component.Coefficients;
            component.Coefficients = []; context.Release(length * 4L);
            context.ReturnCoefficients(storage);
        }
        var result = ImageBuffer.Allocate(info.Width, info.Height, 8, context); result.Orientation = info.Orientation;
        try
        {
            var fullResolutionYcbcr = components.Length == 3 && !rgb;
            if (fullResolutionYcbcr)
                foreach (var component in components)
                    fullResolutionYcbcr &= component.H == maxH && component.V == maxV;
            var fullResolutionRgb = components.Length == 3 && rgb;
            if (fullResolutionRgb)
                foreach (var component in components)
                    fullResolutionRgb &= component.H == maxH && component.V == maxV;
            var horizontalRgb422 = components.Length == 3 && rgb
                && components[0].H == maxH && components[0].V == maxV
                && components[1].H * 2 == maxH && components[1].V == maxV
                && components[2].H == components[1].H && components[2].V == maxV;
            var subsampledRgb420 = components.Length == 3 && rgb
                && components[0].H == maxH && components[0].V == maxV
                && components[1].H * 2 == maxH && components[1].V * 2 == maxV
                && components[2].H == components[1].H && components[2].V == components[1].V;
            var subsampledYcbcr = !rgb && HasHalfResolutionChroma();
            var quarterYcbcr = !rgb && HasQuarterWidthChroma();
            for (var y = 0; y < info.Height; y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var row = result.GetRow8(y);
                if (fullResolutionRgb)
                {
                    // IDCT already rounded/clipped these full-resolution bytes.
                    // Centered interpolation has zero fractions and Round is exact.
                    RenderRgbRow(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                        components[1].Plane.AsSpan(y * components[1].BlockColumns * 8, info.Width),
                        components[2].Plane.AsSpan(y * components[2].BlockColumns * 8, info.Width), row);
                    continue;
                }
                if (horizontalRgb422)
                {
                    // Centered 2:1 coordinates are exact quarters; vertical
                    // coordinates are integral. Keep per-row cancellation checkpoints.
                    RenderRgb422Row(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                        components[1].Plane.AsSpan(y * components[1].BlockColumns * 8, components[1].SampleWidth),
                        components[2].Plane.AsSpan(y * components[2].BlockColumns * 8, components[2].SampleWidth), row);
                    continue;
                }
                if (subsampledRgb420)
                {
                    var half = y / 2; var odd = (y & 1) != 0;
                    var top = Math.Max(odd ? half : half - 1, 0);
                    var bottom = Math.Min(odd ? half + 1 : half, components[1].SampleHeight - 1);
                    var green = components[1]; var blue = components[2];
                    RenderRgb420Row(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                        green.Plane.AsSpan(top * green.BlockColumns * 8, green.SampleWidth),
                        green.Plane.AsSpan(bottom * green.BlockColumns * 8, green.SampleWidth),
                        blue.Plane.AsSpan(top * blue.BlockColumns * 8, blue.SampleWidth),
                        blue.Plane.AsSpan(bottom * blue.BlockColumns * 8, blue.SampleWidth),
                        odd ? 3 : 1, row);
                    continue;
                }
                var x = 0;
                if (components.Length == 1)
                {
                    // A single component always has the full sampling factors.
                    // Its plane already contains rounded/clipped bytes: repeating
                    // that byte in RGB is identical to interpolation and Round.
                    var samples = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width);
                    if (Avx2.IsSupported)
                        for (; x <= info.Width - 8; x += 8)
                        {
                            var packed = Avx2.MultiplyLow(WidenSamples8(samples.Slice(x, 8)), Vector256.Create(0x010101)).AsUInt32();
                            Avx2.Or(packed, Vector256.Create(0xFF000000u)).AsByte().CopyTo(row.Slice(x * 4, 32));
                        }
                    for (; x < info.Width; x++)
                    {
                        var value = samples[x];
                        row[x * 4] = value; row[x * 4 + 1] = value; row[x * 4 + 2] = value; row[x * 4 + 3] = 255;
                    }
                    continue;
                }
                if (fullResolutionYcbcr)
                {
                    JpegInverseColor.WriteRow(
                        components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                        components[1].Plane.AsSpan(y * components[1].BlockColumns * 8, info.Width),
                        components[2].Plane.AsSpan(y * components[2].BlockColumns * 8, info.Width), row, 4);
                    continue;
                }
                if (subsampledYcbcr)
                {
                    WriteYcbcrPixels(y, 0, info.Width, row, 4);
                    continue;
                }
                if (quarterYcbcr)
                {
                    WriteQuarterYcbcrPixels(y, 0, info.Width, row, 4);
                    continue;
                }
                for (; x < info.Width; x++)
                {
                    var first = Interpolate(components[0], x, y);
                    double r = first, g = first, b = first;
                    if (components.Length == 3)
                    {
                        var second = Interpolate(components[1], x, y); var third = Interpolate(components[2], x, y);
                        if (rgb) { g = second; b = third; }
                        else
                        {
                            var cb = second - 128; var cr = third - 128;
                            r = first + 1.402 * cr; g = first - 0.344136286 * cb - 0.714136286 * cr; b = first + 1.772 * cb;
                        }
                    }
                    row[x * 4] = Round(r); row[x * 4 + 1] = Round(g); row[x * 4 + 2] = Round(b); row[x * 4 + 3] = 255;
                }
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private double Interpolate(Component component, int x, int y)
    {
        // Equal sampling factors map the centered coordinates exactly to x/y;
        // both interpolation fractions are zero. The full-resolution plane
        // already covers the output raster, including padded block rows.
        if (component.H == maxH && component.V == maxV)
            return component.Plane[y * component.BlockColumns * 8 + x];
        var cx = (x + 0.5) * component.H / maxH - 0.5; var cy = (y + 0.5) * component.V / maxV - 0.5;
        var ix = (int)Math.Floor(cx); var iy = (int)Math.Floor(cy); var fx = cx - ix; var fy = cy - iy;
        var x0 = Math.Clamp(ix, 0, component.SampleWidth - 1); var x1 = Math.Clamp(ix + 1, 0, component.SampleWidth - 1);
        var y0 = Math.Clamp(iy, 0, component.SampleHeight - 1); var y1 = Math.Clamp(iy + 1, 0, component.SampleHeight - 1);
        var stride = component.BlockColumns * 8;
        var top = component.Plane[y0 * stride + x0] * (1 - fx) + component.Plane[y0 * stride + x1] * fx;
        var bottom = component.Plane[y1 * stride + x0] * (1 - fx) + component.Plane[y1 * stride + x1] * fx;
        return top * (1 - fy) + bottom * fy;
    }

    private static byte Round(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);

    internal static void RenderRgb420Row(ReadOnlySpan<byte> red, ReadOnlySpan<byte> greenTop, ReadOnlySpan<byte> greenBottom,
        ReadOnlySpan<byte> blueTop, ReadOnlySpan<byte> blueBottom, int topWeight, Span<byte> rgba)
    {
        if (greenTop.Length != (red.Length + 1L) / 2 || greenBottom.Length != greenTop.Length
            || blueTop.Length != greenTop.Length || blueBottom.Length != greenTop.Length
            || topWeight is not (1 or 3) || rgba.Length != red.Length * 4L)
            throw new ArgumentException("RGB420 planes, quarter weights and RGBA row lengths differ.");
        var x = 0;
        for (; x < Math.Min(2, red.Length); x++)
        {
            var neighbor = Math.Min(x, greenTop.Length - 1);
            rgba[x * 4] = red[x];
            rgba[x * 4 + 1] = Channel(greenTop, greenBottom, 0, neighbor, topWeight);
            rgba[x * 4 + 2] = Channel(blueTop, blueBottom, 0, neighbor, topWeight); rgba[x * 4 + 3] = 255;
        }
        if (Avx2.IsSupported)
        {
            var centers = Vector256.Create(1, 1, 2, 2, 3, 3, 4, 4);
            var neighbors = Vector256.Create(0, 2, 1, 3, 2, 4, 3, 5);
            var upperWeight = Vector256.Create(topWeight); var lowerWeight = Vector256.Create(4 - topWeight);
            for (; x <= red.Length - 8 && x / 2 <= greenTop.Length - 7; x += 8)
            {
                var offset = x / 2 - 1;
                // Preserve fractional vertical samples until the one final
                // rounding. Centered 2x2 interpolation has denominator sixteen.
                var gv = Avx2.Add(Avx2.MultiplyLow(WidenSamples8(greenTop.Slice(offset, 8)), upperWeight),
                    Avx2.MultiplyLow(WidenSamples8(greenBottom.Slice(offset, 8)), lowerWeight));
                var bv = Avx2.Add(Avx2.MultiplyLow(WidenSamples8(blueTop.Slice(offset, 8)), upperWeight),
                    Avx2.MultiplyLow(WidenSamples8(blueBottom.Slice(offset, 8)), lowerWeight));
                var gc = Avx2.PermuteVar8x32(gv, centers); var bc = Avx2.PermuteVar8x32(bv, centers);
                var g = Avx2.ShiftRightLogical(Avx2.Add(Avx2.Add(Avx2.Add(gc, Avx2.ShiftLeftLogical(gc, 1)),
                    Avx2.PermuteVar8x32(gv, neighbors)), Vector256.Create(8)), 4).AsUInt32();
                var b = Avx2.ShiftRightLogical(Avx2.Add(Avx2.Add(Avx2.Add(bc, Avx2.ShiftLeftLogical(bc, 1)),
                    Avx2.PermuteVar8x32(bv, neighbors)), Vector256.Create(8)), 4).AsUInt32();
                var r = WidenSamples8(red.Slice(x, 8)).AsUInt32();
                Avx2.Or(Avx2.Or(r, Avx2.ShiftLeftLogical(g, 8)),
                    Avx2.Or(Avx2.ShiftLeftLogical(b, 16), Vector256.Create(0xFF000000u))).AsByte().CopyTo(rgba.Slice(x * 4, 32));
            }
        }
        for (; x < red.Length; x++)
        {
            var k = x / 2;
            var neighbor = (x & 1) == 0 ? Math.Max(k - 1, 0) : Math.Min(k + 1, greenTop.Length - 1);
            rgba[x * 4] = red[x]; rgba[x * 4 + 1] = Channel(greenTop, greenBottom, k, neighbor, topWeight);
            rgba[x * 4 + 2] = Channel(blueTop, blueBottom, k, neighbor, topWeight); rgba[x * 4 + 3] = 255;
        }
        static byte Channel(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom, int k, int neighbor, int weight)
            => (byte)((weight * (3 * top[k] + top[neighbor]) + (4 - weight) * (3 * bottom[k] + bottom[neighbor]) + 8) >> 4);
    }

    internal static void RenderRgb422Row(ReadOnlySpan<byte> red, ReadOnlySpan<byte> green, ReadOnlySpan<byte> blue, Span<byte> rgba)
    {
        if (green.Length != (red.Length + 1L) / 2 || blue.Length != green.Length || rgba.Length != red.Length * 4L)
            throw new ArgumentException("RGB422 planes and RGBA row lengths differ.");
        var x = 0;
        if (Avx2.IsSupported && Ssse3.IsSupported && green.Length >= 8)
        {
            rgba[0] = red[0]; rgba[1] = green[0]; rgba[2] = blue[0]; rgba[3] = 255;
            rgba[4] = red[1]; rgba[5] = (byte)((3 * green[0] + green[1] + 2) >> 2);
            rgba[6] = (byte)((3 * blue[0] + blue[1] + 2) >> 2); rgba[7] = 255;
            x = 2;
            // Four component samples supply eight output pixels. The neighbor
            // alternates left/right; these are channel indices, not codec tables.
            var centers = Vector128.Create((byte)1, 1, 2, 2, 3, 3, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0);
            var neighbors = Vector128.Create((byte)0, 2, 1, 3, 2, 4, 3, 5, 0, 0, 0, 0, 0, 0, 0, 0);
            for (; x <= red.Length - 8 && x / 2 <= green.Length - 7; x += 8)
            {
                var offset = x / 2 - 1;
                var gs = Vector128.Create(Vector64.Create(green.Slice(offset, 8)), Vector64<byte>.Zero);
                var bs = Vector128.Create(Vector64.Create(blue.Slice(offset, 8)), Vector64<byte>.Zero);
                var gc = Avx2.ConvertToVector256Int32(Ssse3.Shuffle(gs, centers));
                var bc = Avx2.ConvertToVector256Int32(Ssse3.Shuffle(bs, centers));
                var g = Avx2.ShiftRightLogical(Avx2.Add(Avx2.Add(Avx2.Add(gc, Avx2.ShiftLeftLogical(gc, 1)),
                    Avx2.ConvertToVector256Int32(Ssse3.Shuffle(gs, neighbors))), Vector256.Create(2)), 2).AsUInt32();
                var b = Avx2.ShiftRightLogical(Avx2.Add(Avx2.Add(Avx2.Add(bc, Avx2.ShiftLeftLogical(bc, 1)),
                    Avx2.ConvertToVector256Int32(Ssse3.Shuffle(bs, neighbors))), Vector256.Create(2)), 2).AsUInt32();
                var r = WidenSamples8(red.Slice(x, 8)).AsUInt32();
                Avx2.Or(Avx2.Or(r, Avx2.ShiftLeftLogical(g, 8)),
                    Avx2.Or(Avx2.ShiftLeftLogical(b, 16), Vector256.Create(0xFF000000u))).AsByte().CopyTo(rgba.Slice(x * 4, 32));
            }
        }
        for (; x < red.Length; x++)
        {
            var k = x / 2;
            var neighbor = (x & 1) == 0 ? Math.Max(k - 1, 0) : Math.Min(k + 1, green.Length - 1);
            rgba[x * 4] = red[x]; rgba[x * 4 + 1] = (byte)((3 * green[k] + green[neighbor] + 2) >> 2);
            rgba[x * 4 + 2] = (byte)((3 * blue[k] + blue[neighbor] + 2) >> 2); rgba[x * 4 + 3] = 255;
        }
    }

    internal static void RenderRgbRow(ReadOnlySpan<byte> red, ReadOnlySpan<byte> green, ReadOnlySpan<byte> blue, Span<byte> rgba)
    {
        if (green.Length != red.Length || blue.Length != red.Length || rgba.Length != red.Length * 4L)
            throw new ArgumentException("RGB planes and RGBA row lengths differ.");
        var x = 0;
        if (Avx2.IsSupported)
            for (; x <= red.Length - 8; x += 8)
            {
                var r = WidenSamples8(red.Slice(x, 8)).AsUInt32();
                var g = Avx2.ShiftLeftLogical(WidenSamples8(green.Slice(x, 8)).AsUInt32(), 8);
                var b = Avx2.ShiftLeftLogical(WidenSamples8(blue.Slice(x, 8)).AsUInt32(), 16);
                Avx2.Or(Avx2.Or(r, g), Avx2.Or(b, Vector256.Create(0xFF000000u))).AsByte().CopyTo(rgba.Slice(x * 4, 32));
            }
        for (; x < red.Length; x++)
        {
            rgba[x * 4] = red[x]; rgba[x * 4 + 1] = green[x]; rgba[x * 4 + 2] = blue[x]; rgba[x * 4 + 3] = 255;
        }
    }

    private static Vector256<int> WidenSamples8(ReadOnlySpan<byte> samples)
        => Avx2.ConvertToVector256Int32(Vector128.Create(Vector64.Create(samples), Vector64<byte>.Zero));

}
