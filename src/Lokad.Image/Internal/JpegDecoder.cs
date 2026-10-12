using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Owned implementation of T.81 Annexes B, F and G for 8-bit Huffman JPEG.</summary>
internal ref struct JpegDecoder
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

    public JpegDecoder(ReadOnlySpan<byte> data, OperationContext context)
    {
        this.data = data; this.context = context;
        info = ContainerHeaders.Jpeg(data, context);
        context.Reserve(1024); // Fixed parser references and component descriptors.
        quantization = new ushort[4][]; quantizationPrecision = new byte[4];
        dcTables = new JpegHuffman[4]; acTables = new JpegHuffman[4];
        components = []; position = 2; maxH = 0; maxV = 0; mcuColumns = 0; mcuRows = 0; restartInterval = 0;
        progressive = false; frameSeen = false; scanSeen = false; rgb = false;
    }

    public ImageBuffer Decode()
    {
        while (position < data.Length)
        {
            context.Work(1);
            var marker = ReadMarker();
            if (marker == 0xD9)
            {
                if (!scanSeen) throw new ImageDataException("JPEG has no pixel scans.");
                foreach (var component in components)
                    if (component.Approximation[0] < 0) throw new ImageDataException("JPEG component is missing a DC scan.");
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
                    if (payload[11] > 1) throw new UnsupportedImageException("Unsupported JPEG Adobe color transform.");
                    rgb = payload[11] == 0; break;
                case >= 0xE0 and <= 0xEF or 0xFE: break;
                default: throw new UnsupportedImageException("Unsupported JPEG control marker.");
            }
        }
        throw new ImageDataException("JPEG is missing its end marker.");
    }

    private int ReadMarker()
    {
        if (position >= data.Length || data[position++] != 255) throw new ImageDataException("Invalid JPEG marker.");
        while (position < data.Length && data[position] == 255) { context.Work(1); position++; }
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
        context.Work(length);
        return payload;
    }

    private void ReadFrame(ReadOnlySpan<byte> payload, bool isProgressive)
    {
        if (frameSeen || payload.Length < 6) throw new ImageDataException("Invalid JPEG frame.");
        progressive = isProgressive; frameSeen = true;
        var count = payload[5];
        if (count is not (1 or 3) || payload.Length != 6 + count * 3) throw new ImageDataException("Invalid JPEG components.");
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
        else if (components[0].H != maxH || components[0].V != maxV
            || components[1].H != components[2].H || components[1].V != components[2].V
            || maxH % components[1].H != 0 || maxV % components[1].V != 0
            || maxH / components[1].H is not (1 or 2)
            || maxV / components[1].V is not (1 or 2)
            || (maxH == components[1].H && maxV != components[1].V))
            throw new UnsupportedImageException("Unsupported JPEG component sampling layout.");
        if (count == 3 && components[0].Id == 'R' && components[1].Id == 'G' && components[2].Id == 'B') rgb = true;
        mcuColumns = (info.Width + 8 * maxH - 1) / (8 * maxH);
        mcuRows = (info.Height + 8 * maxV - 1) / (8 * maxV);
        foreach (var component in components)
        {
            component.BlockColumns = mcuColumns * component.H; component.BlockRows = mcuRows * component.V;
            component.SampleWidth = (info.Width * component.H + maxH - 1) / maxH;
            component.SampleHeight = (info.Height * component.V + maxV - 1) / maxV;
            var countValues = (long)component.BlockColumns * component.BlockRows * 64;
            if (countValues > Array.MaxLength) throw new ImageLimitException("JPEG coefficient plane is too large.");
            context.Reserve(countValues * 4 + 64 * 4);
            component.Coefficients = new int[(int)countValues];
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
        if (!frameSeen || payload.Length < 6 || payload[0] is < 1 or > 3 || payload.Length != 4 + payload[0] * 2)
            throw new ImageDataException("Invalid JPEG scan selectors.");
        var count = payload[0];
        Span<int> indexes = stackalloc int[3]; Span<int> dc = stackalloc int[3]; Span<int> ac = stackalloc int[3];
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
            context.Work(1);
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
        context.Work(64);
        reader.BeginLocalWork();
        try
        {
            var category = dc.Read(ref reader);
            if (category > 11) throw new ImageDataException("Invalid JPEG DC coefficient category.");
            var nextPredictor = (long)predictor + reader.ReadSigned(category);
            if (nextPredictor is < -32768 or > 32767) throw new ImageDataException("JPEG DC predictor is out of range.");
            predictor = (int)nextPredictor; block[0] = predictor;
            var k = 1;
            while (k <= 63)
            {
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
        finally { reader.EndLocalWork(); }
    }

    private void ReadBlock(ref JpegEntropyReader reader, Span<int> block, Component component,
        JpegHuffman? dc, JpegHuffman? ac, int ss, int se, int ah, int al, ref int eobRun)
    {
        context.Work(64);
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
                block[JpegTransform.Zigzag[k++]] = reader.ReadSigned(size) << al;
            }
            return;
        }

        // Successive-approximation AC decoding: existing nonzero coefficients
        // consume correction bits while run counts count only zero coefficients.
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
                    ref var value = ref block[JpegTransform.Zigzag[positionInBand]];
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
                if (newValue != 0) block[JpegTransform.Zigzag[positionInBand++]] = newValue;
            }
        }
        if (eobRun > 0)
        {
            while (positionInBand <= se)
            {
                ref var value = ref block[JpegTransform.Zigzag[positionInBand++]];
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
                JpegTransform.Inverse(component.Coefficients.AsSpan((y * component.BlockColumns + x) * 64, 64),
                    component.FrozenQuantization!, block, context);
                for (var by = 0; by < 8; by++)
                    block.Slice(by * 8, 8).CopyTo(component.Plane.AsSpan((y * 8 + by) * component.BlockColumns * 8 + x * 8, 8));
            }
            component.Coefficients = []; context.Release(length * 4L);
        }
        var result = ImageBuffer.Allocate(info.Width, info.Height, 8, context); result.Orientation = info.Orientation;
        try
        {
            var fullResolutionYcbcr = components.Length == 3 && !rgb
                && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported;
            if (fullResolutionYcbcr)
                foreach (var component in components)
                    fullResolutionYcbcr &= component.H == maxH && component.V == maxV;
            var subsampledYcbcr = components.Length == 3 && !rgb
                && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported
                && components[0].H == maxH && components[0].V == maxV
                && components[1].H * 2 == maxH && components[1].V * 2 == maxV
                && components[2].H == components[1].H && components[2].V == components[1].V;
            for (var y = 0; y < info.Height; y++)
            {
                context.Work(info.Width * 32L);
                var row = result.GetRow8(y);
                var x = 0;
                var subsampledEnd = 2;
                if (fullResolutionYcbcr)
                {
                    var first = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width);
                    var second = components[1].Plane.AsSpan(y * components[1].BlockColumns * 8, info.Width);
                    var third = components[2].Plane.AsSpan(y * components[2].BlockColumns * 8, info.Width);
                    for (; x <= info.Width - 8; x += 8)
                        RenderYcbcrRow8(first.Slice(x, 8), second.Slice(x, 8), third.Slice(x, 8), row.Slice(x * 4, 32), block[..24]);
                }
                if (subsampledYcbcr)
                {
                    // At 2:1 centered sampling the interpolation fractions are
                    // exactly 1/4 or 3/4. Share row geometry across both chroma
                    // planes; leave edge pixels to the general clamp policy.
                    var chroma = components[1]; var stride = chroma.BlockColumns * 8;
                    var iy = (y - 1) >> 1;
                    var y0 = Math.Clamp(iy, 0, chroma.SampleHeight - 1);
                    var y1 = Math.Clamp(iy + 1, 0, chroma.SampleHeight - 1);
                    var topWeight = (y & 1) == 0 ? 1 : 3;
                    // The first two pixels use the original scalar path below.
                    // Start SIMD at the next even pixel, then retain scalar tails.
                    for (var start = 2; start <= info.Width - 8; start += 8)
                    {
                        var column = start / 2 - 1;
                        if (column > stride - 8 || column > chroma.SampleWidth - 6) break;
                        RenderYcbcr420Row8(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8 + start, 8),
                            chroma.Plane.AsSpan(y0 * stride + column, 8), chroma.Plane.AsSpan(y1 * stride + column, 8),
                            components[2].Plane.AsSpan(y0 * stride + column, 8), components[2].Plane.AsSpan(y1 * stride + column, 8),
                            topWeight, row.Slice(start * 4, 32), block[..24]);
                        subsampledEnd = start + 8;
                    }
                }
                for (; x < info.Width; x++)
                {
                    if (x == 2 && subsampledEnd > 2)
                    {
                        x = subsampledEnd;
                        if (x >= info.Width) break;
                    }
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

    internal static void RenderYcbcrRow8(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second, ReadOnlySpan<byte> third,
        Span<byte> rgba, Span<byte> scratch)
    {
        if (first.Length != 8 || second.Length != 8 || third.Length != 8 || rgba.Length != 32 || scratch.Length != 24)
            throw new ArgumentException("JPEG color rows have eight component samples, eight RGBA pixels and 24 scratch bytes.");
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            // Retain each separate operation of the owned full-resolution
            // binary64 YCbCr policy. The already used inverse-block workspace
            // holds three rounded rows; no new scratch/cache is needed.
            var luma = Avx512F.ConvertToVector512Double(WidenSamples8(first));
            var cb = Avx512F.ConvertToVector512Double(Avx2.Subtract(WidenSamples8(second), Vector256.Create(128)));
            var cr = Avx512F.ConvertToVector512Double(Avx2.Subtract(WidenSamples8(third), Vector256.Create(128)));
            JpegTransform.RoundSamples(Avx512F.Add(luma, Avx512F.Multiply(Vector512.Create(1.402), cr)), scratch[..8]);
            JpegTransform.RoundSamples(Avx512F.Subtract(Avx512F.Subtract(luma, Avx512F.Multiply(Vector512.Create(0.344136286), cb)),
                Avx512F.Multiply(Vector512.Create(0.714136286), cr)), scratch.Slice(8, 8));
            JpegTransform.RoundSamples(Avx512F.Add(luma, Avx512F.Multiply(Vector512.Create(1.772), cb)), scratch.Slice(16, 8));
            var red = WidenSamples8(scratch[..8]);
            var green = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(8, 8)).AsUInt32(), 8);
            var blue = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(16, 8)).AsUInt32(), 16);
            Avx2.Or(Avx2.Or(red.AsUInt32(), green), Avx2.Or(blue, Vector256.Create(0xFF000000u))).AsByte().CopyTo(rgba);
            return;
        }
        for (var x = 0; x < 8; x++)
        {
            var cb = second[x] - 128; var cr = third[x] - 128;
            rgba[x * 4] = Round(first[x] + 1.402 * cr);
            rgba[x * 4 + 1] = Round(first[x] - 0.344136286 * cb - 0.714136286 * cr);
            rgba[x * 4 + 2] = Round(first[x] + 1.772 * cb); rgba[x * 4 + 3] = 255;
        }
    }

    private static Vector256<int> WidenSamples8(ReadOnlySpan<byte> samples)
        => Avx2.ConvertToVector256Int32(Vector128.Create(Vector64.Create(samples), Vector64<byte>.Zero));

    internal static void RenderYcbcr420Row8(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom,
        ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom, int topWeight, Span<byte> rgba, Span<byte> scratch)
    {
        if (luma.Length != 8 || cbTop.Length != 8 || cbBottom.Length != 8 || crTop.Length != 8 || crBottom.Length != 8
            || topWeight is not (1 or 3) || rgba.Length != 32 || scratch.Length != 24)
            throw new ArgumentException("JPEG 4:2:0 rows have eight samples, quarter weights, eight RGBA pixels and 24 scratch bytes.");
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            var first = Avx512F.ConvertToVector512Double(WidenSamples8(luma));
            var cb = Interpolate420Row8(cbTop, cbBottom, topWeight);
            var cr = Interpolate420Row8(crTop, crBottom, topWeight);
            JpegTransform.RoundSamples(Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.402), cr)), scratch[..8]);
            JpegTransform.RoundSamples(Avx512F.Subtract(Avx512F.Subtract(first, Avx512F.Multiply(Vector512.Create(0.344136286), cb)),
                Avx512F.Multiply(Vector512.Create(0.714136286), cr)), scratch.Slice(8, 8));
            JpegTransform.RoundSamples(Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.772), cb)), scratch.Slice(16, 8));
            var red = WidenSamples8(scratch[..8]);
            var green = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(8, 8)).AsUInt32(), 8);
            var blue = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(16, 8)).AsUInt32(), 16);
            Avx2.Or(Avx2.Or(red.AsUInt32(), green), Avx2.Or(blue, Vector256.Create(0xFF000000u))).AsByte().CopyTo(rgba);
            return;
        }
        for (var x = 0; x < 8; x++)
        {
            var left = x / 2 + (x & 1); var right = left + 1;
            var fx = (x & 1) == 0 ? 0.75 : 0.25;
            var top = topWeight / 4d; var bottom = (4 - topWeight) / 4d;
            var cb = (cbTop[left] * (1 - fx) + cbTop[right] * fx) * top
                + (cbBottom[left] * (1 - fx) + cbBottom[right] * fx) * bottom - 128;
            var cr = (crTop[left] * (1 - fx) + crTop[right] * fx) * top
                + (crBottom[left] * (1 - fx) + crBottom[right] * fx) * bottom - 128;
            rgba[x * 4] = Round(luma[x] + 1.402 * cr);
            rgba[x * 4 + 1] = Round(luma[x] - 0.344136286 * cb - 0.714136286 * cr);
            rgba[x * 4 + 2] = Round(luma[x] + 1.772 * cb); rgba[x * 4 + 3] = 255;
        }
    }

    private static Vector512<double> Interpolate420Row8(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom, int topWeight)
    {
        // T.81 sampling maps these centered pixels to exact quarter fractions.
        // Integer numerators reproduce the binary64 horizontal/vertical sums
        // exactly: their maximum is 16*255. No chroma rounding is introduced.
        var left = Vector256.Create(0, 1, 1, 2, 2, 3, 3, 4);
        var right = Vector256.Create(1, 2, 2, 3, 3, 4, 4, 5);
        var a = Vector256.Create(1, 3, 1, 3, 1, 3, 1, 3);
        var b = Vector256.Create(3, 1, 3, 1, 3, 1, 3, 1);
        var upper = WidenSamples8(top); var lower = WidenSamples8(bottom);
        upper = Avx2.Add(Avx2.MultiplyLow(Avx2.PermuteVar8x32(upper, left), a), Avx2.MultiplyLow(Avx2.PermuteVar8x32(upper, right), b));
        lower = Avx2.Add(Avx2.MultiplyLow(Avx2.PermuteVar8x32(lower, left), a), Avx2.MultiplyLow(Avx2.PermuteVar8x32(lower, right), b));
        var sum = Avx2.Add(Avx2.MultiplyLow(upper, Vector256.Create(topWeight)), Avx2.MultiplyLow(lower, Vector256.Create(4 - topWeight)));
        return Avx512F.Multiply(Avx512F.ConvertToVector512Double(Avx2.Subtract(sum, Vector256.Create(128 * 16))), Vector512.Create(1d / 16));
    }
}
