using System.Buffers.Binary;

namespace Lokad.Image.Internal;

// Independent streaming header grammar: PNG3 chunks, T.81 marker framing,
// RFC9649 RIFF/VP8 headers and CIPA Exif IFD0. Pixel syntax and PNG CRCs are opaque.
internal static class StreamInspection
{
    internal static async ValueTask<TResult> InspectAsync<TResult, TFactory>(InspectionReader r, ProbeReader? probe = null,
        ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
        where TResult : struct where TFactory : struct, IInspectionResult<TResult>
    {
        await r.ReadAsync(2).ConfigureAwait(false);
        if (r.Header[0] == 137 && r.Header[1] == 80)
        {
            await r.ReadAsync(6).ConfigureAwait(false);
            if (!r.Header.AsSpan(0, 6).SequenceEqual(PngCodec.Signature[2..])) throw new ImageDataException("Invalid PNG signature.");
            return await PngAsync<TResult, TFactory>(r, probe, metadataPolicy).ConfigureAwait(false);
        }
        if (r.Header[0] == 255 && r.Header[1] == 216) return await JpegAsync<TResult, TFactory>(r, probe, metadataPolicy).ConfigureAwait(false);
        if (r.Header[0] == 'R' && r.Header[1] == 'I')
        {
            await r.ReadAsync(10).ConfigureAwait(false);
            if (!r.Header.AsSpan(0, 2).SequenceEqual("FF"u8) || !r.Header.AsSpan(6, 4).SequenceEqual("WEBP"u8)) throw new ImageDataException("Invalid WebP RIFF header.");
            return await WebpAsync<TResult, TFactory>(r, BinaryPrimitives.ReadUInt32LittleEndian(r.Header.AsSpan(2)) + 8L, probe, metadataPolicy).ConfigureAwait(false);
        }
        throw new UnsupportedImageException(UnsupportedImageFeature.Format, "Expected a still PNG, JPEG or WebP image.");
    }

    private static uint Four(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    private static ushort U16(ReadOnlySpan<byte> bytes, bool little) => little ? BinaryPrimitives.ReadUInt16LittleEndian(bytes) : BinaryPrimitives.ReadUInt16BigEndian(bytes);
    private static uint U32(ReadOnlySpan<byte> bytes, bool little) => little ? BinaryPrimitives.ReadUInt32LittleEndian(bytes) : BinaryPrimitives.ReadUInt32BigEndian(bytes);
    private static int U24(ReadOnlySpan<byte> bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16;
    private static void Item(InspectionReader r, ref int items)
    {
        r.Context.Cancellation.ThrowIfCancellationRequested();
        if (++items > r.Context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Image container item limit exceeded.");
    }
    private static void Metadata(InspectionReader r, long bytes, ref long total)
    {
        if (bytes > r.Context.Limits.MaxMetadataBytes - total) throw new ImageLimitException(ImageLimitKind.MetadataBytes, "Image metadata limit exceeded.");
        total += bytes;
    }
    private static void Dimensions(InspectionReader r, int width, int height)
    {
        if (width < 1 || height < 1) throw new ImageDataException("Invalid image dimensions.");
        if (width > r.Context.Limits.MaxDimension || height > r.Context.Limits.MaxDimension)
            throw new ImageLimitException(ImageLimitKind.Dimension, "Image dimensions exceed the configured limit.");
        if ((long)width * height > r.Context.Limits.MaxPixels)
            throw new ImageLimitException(ImageLimitKind.PixelCount, "Image pixel count exceeds the configured limit.");
        // No decoded array exists here, so no hypothetical RGBA array ceiling.
    }
    private static void PngName(ReadOnlySpan<byte> type)
    {
        foreach (var b in type) if (b is not (>= 65 and <= 90) and not (>= 97 and <= 122)) throw new ImageDataException("Invalid PNG chunk name.");
    }

    private static async ValueTask<TResult> PngAsync<TResult, TFactory>(InspectionReader r, ProbeReader? probe, ImageMetadataPolicy metadataPolicy)
        where TResult : struct where TFactory : struct, IInspectionResult<TResult>
    {
        int items = 0, width = 0, height = 0, depth = 0, color = 0, palette = 0;
        long metadata = 0, compressed = 0;
        bool header = false, data = false, closed = false, transparency = false, exif = false;
        var orientation = ImageOrientation.Normal;
        while (true)
        {
            Item(r, ref items); await r.ReadAsync(8).ConfigureAwait(false);
            var size = BinaryPrimitives.ReadUInt32BigEndian(r.Header); var type = Four(r.Header.AsSpan(4)); PngName(r.Header.AsSpan(4, 4));
            if (size > int.MaxValue) throw new ImageDataException("Invalid PNG chunk length.");
            var end = r.Position + size; r.DeclaredEnd(end + 4);
            if (!header && type != Four("IHDR"u8)) throw new ImageDataException("PNG must start with IHDR.");
            if (data && type != Four("IDAT"u8)) closed = true;
            if (type == Four("IHDR"u8))
            {
                if (header || size != 13) throw new ImageDataException("Invalid PNG IHDR.");
                await r.ReadAsync(13).ConfigureAwait(false);
                var w = BinaryPrimitives.ReadUInt32BigEndian(r.Header); var h = BinaryPrimitives.ReadUInt32BigEndian(r.Header.AsSpan(4));
                if (w is 0 or > int.MaxValue || h is 0 or > int.MaxValue) throw new ImageDataException("Invalid PNG dimensions.");
                width = (int)w; height = (int)h; depth = r.Header[8]; color = r.Header[9];
                var valid = color switch { 0 => depth is 1 or 2 or 4 or 8 or 16, 2 or 4 or 6 => depth is 8 or 16, 3 => depth is 1 or 2 or 4 or 8, _ => false };
                if (!valid || r.Header[10] != 0 || r.Header[11] != 0 || r.Header[12] > 1) throw new ImageDataException("Invalid PNG coding profile.");
                Dimensions(r, width, height); header = true;
                if (probe?.Reached(ImageProbeFacts.Dimensions | ImageProbeFacts.SourceRepresentation
                    | (color is 4 or 6 ? ImageProbeFacts.Transparency : 0)) == true)
                    return ImageSourceMetadata.Png<TResult, TFactory>(probe, width, height, depth, color, transparency, orientation);
            }
            else if (type == Four("PLTE"u8))
            {
                if (palette != 0 || data || transparency || color is 0 or 4 || size is < 3 or > 768 || size % 3 != 0 || color == 3 && size / 3 > 1 << depth)
                    throw new ImageDataException("Invalid PNG palette.");
                palette = (int)size;
            }
            else if (type == Four("tRNS"u8))
            {
                if (transparency || data || color == 3 && (palette == 0 || size == 0 || size > palette / 3)
                    || color == 0 && size != 2 || color == 2 && size != 6 || color is 4 or 6) throw new ImageDataException("Invalid PNG transparency chunk.");
                transparency = true;
                if (color is 0 or 2)
                {
                    await r.ReadAsync((int)size).ConfigureAwait(false);
                    for (var i = 0; i < size; i += 2) if (BinaryPrimitives.ReadUInt16BigEndian(r.Header.AsSpan(i)) > (1 << depth) - 1) throw new ImageDataException("PNG transparency sample exceeds bit depth.");
                }
                if (probe?.Reached(ImageProbeFacts.Transparency) == true)
                    return ImageSourceMetadata.Png<TResult, TFactory>(probe, width, height, depth, color, transparency, orientation);
            }
            else if (type == Four("IDAT"u8))
            {
                if (closed || color == 3 && palette == 0) throw new ImageDataException("Invalid PNG IDAT ordering.");
                data = true; compressed += size;
                // In a valid PNG these facts are settled before the first IDAT.
                if (probe is not null)
                {
                    if (!exif && probe.WantsOrientation && metadataPolicy == ImageMetadataPolicy.Strict) probe.Orientation();
                    probe.Reached(ImageProbeFacts.Animation);
                    if (probe.Reached(ImageProbeFacts.Transparency))
                        return ImageSourceMetadata.Png<TResult, TFactory>(probe, width, height, depth, color, transparency, orientation);
                }
            }
            else if (type == Four("IEND"u8))
            {
                if (size != 0 || !data || compressed == 0) throw new ImageDataException("Invalid PNG end.");
                await r.SkipAsync(4).ConfigureAwait(false); await r.EndAsync().ConfigureAwait(false);
                if (probe?.WantsOrientation == true) probe.Orientation();
                probe?.Reached(ImageProbeFacts.All);
                return ImageSourceMetadata.Png<TResult, TFactory>(probe, width, height, depth, color, transparency, orientation);
            }
            else if (type == Four("acTL"u8) || type == Four("fcTL"u8) || type == Four("fdAT"u8))
            {
                if (probe is null) throw new UnsupportedImageException(UnsupportedImageFeature.Animation, "Animated PNG is not supported.", ImageFormat.Png);
                Metadata(r, size, ref metadata);
                if (type == Four("acTL"u8))
                {
                    if (data || size != 8) throw new ImageDataException("Invalid PNG animation control.");
                    await r.ReadAsync(8).ConfigureAwait(false);
                    if (BinaryPrimitives.ReadUInt32BigEndian(r.Header) == 0) throw new ImageDataException("Invalid PNG animation frame count.");
                }
                probe.Animation(true);
                if (probe.Reached(0)) return ImageSourceMetadata.Png<TResult, TFactory>(probe, width, height, depth, color, transparency, orientation);
            }
            else
            {
                if ((type & 32) == 0) throw new UnsupportedImageException(UnsupportedImageFeature.CriticalChunk, "Unknown critical PNG chunk.", ImageFormat.Png);
                Metadata(r, size, ref metadata);
                if (type == Four("eXIf"u8) && (probe is null || probe.WantsOrientation))
                {
                    if ((exif || data) && metadataPolicy == ImageMetadataPolicy.Strict) throw new ImageDataException("Duplicate or misplaced PNG EXIF chunk.");
                    orientation = (await ExifAsync(r, size, true, metadataPolicy).ConfigureAwait(false)) ?? orientation; exif = true;
                    if (metadataPolicy == ImageMetadataPolicy.Strict) probe?.Orientation();
                    if (probe?.Reached(0) == true) return ImageSourceMetadata.Png<TResult, TFactory>(probe, width, height, depth, color, transparency, orientation);
                }
            }
            await r.SkipAsync(end - r.Position).ConfigureAwait(false);
            await r.SkipAsync(4).ConfigureAwait(false); // CRC bytes are intentionally opaque.
        }
    }

    private static ValueTask<TResult> JpegAsync<TResult, TFactory>(InspectionReader r, ProbeReader? probe, ImageMetadataPolicy metadataPolicy)
        where TResult : struct where TFactory : struct, IInspectionResult<TResult>
    {
        var state = new JpegInspectionState { Orientation = ImageOrientation.Normal, Source = new JpegSourceMetadata() };
        return AdvanceJpeg(r, probe, metadataPolicy, ref state, out var pending)
            ? ValueTask.FromResult(state.Source.Info<TResult, TFactory>(probe, state.Width, state.Height, state.Orientation))
            : AwaitJpegAsync<TResult, TFactory>(r, probe, metadataPolicy, state, pending);
    }

    // The grammar advances synchronously until an actual incomplete operation.
    // Public Probe/InspectHeaders wrappers own the reader through every suspension.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static async ValueTask<TResult> AwaitJpegAsync<TResult, TFactory>(InspectionReader r, ProbeReader? probe,
        ImageMetadataPolicy metadataPolicy, JpegInspectionState state, JpegPending pending)
        where TResult : struct where TFactory : struct, IInspectionResult<TResult>
    {
        do
        {
            switch (pending.Kind)
            {
                case JpegPendingKind.Marker: state.Marker = await pending.Marker.ConfigureAwait(false); break;
                case JpegPendingKind.Exif: state.Orientation = (await pending.Exif.ConfigureAwait(false)) ?? state.Orientation; break;
                default: await pending.Operation.ConfigureAwait(false); break;
            }
            pending = default;
        } while (!AdvanceJpeg(r, probe, metadataPolicy, ref state, out pending));
        return state.Source.Info<TResult, TFactory>(probe, state.Width, state.Height, state.Orientation);
    }

    private enum JpegResume { Start, Marker, Length, Frame, Components, Scan, Application, ExifPrefix, Exif, End }
    private enum JpegPendingKind { Operation, Marker, Exif }

    private struct JpegInspectionState
    {
        internal int Items, Width, Height, Marker, Size, Count;
        internal long Metadata, End;
        internal bool Frame, Scan, ScanSeen, Exif;
        internal ImageOrientation Orientation;
        internal JpegSourceMetadata Source;
        internal JpegResume Resume;
    }

    private readonly struct JpegPending
    {
        internal readonly JpegPendingKind Kind;
        internal readonly ValueTask Operation;
        internal readonly ValueTask<int> Marker;
        internal readonly ValueTask<ImageOrientation?> Exif;

        internal JpegPending(ValueTask operation) { Kind = JpegPendingKind.Operation; Operation = operation; }
        internal JpegPending(ValueTask<int> marker) { Kind = JpegPendingKind.Marker; Marker = marker; }
        internal JpegPending(ValueTask<ImageOrientation?> exif) { Kind = JpegPendingKind.Exif; Exif = exif; }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool CompleteJpegOperation(ValueTask operation, JpegResume resume,
        ref JpegInspectionState state, ref JpegPending pending)
    {
        if (operation.IsCompletedSuccessfully) { operation.GetAwaiter().GetResult(); return true; }
        state.Resume = resume; pending = new JpegPending(operation); return false;
    }

    private static bool AdvanceJpeg(InspectionReader r, ProbeReader? probe, ImageMetadataPolicy metadataPolicy,
        ref JpegInspectionState state, out JpegPending pending)
    {
        pending = default;
        switch (state.Resume)
        {
            case JpegResume.Marker: goto Marker;
            case JpegResume.Length: goto Length;
            case JpegResume.Frame: goto Frame;
            case JpegResume.Components: goto Components;
            case JpegResume.Scan: goto Scan;
            case JpegResume.Application: goto Application;
            case JpegResume.ExifPrefix: goto ExifPrefix;
            case JpegResume.Exif: goto Exif;
            case JpegResume.End: goto End;
        }

    NextMarker:
        var marker = state.Scan ? r.JpegMarkerAsync(true) : r.JpegHeaderMarkerAsync();
        if (!marker.IsCompletedSuccessfully)
        {
            state.Resume = JpegResume.Marker; pending = new JpegPending(marker); return false;
        }
        state.Marker = marker.Result;
    Marker:
        if (state.Scan && state.Marker == 0) goto NextMarker;
        Item(r, ref state.Items);
        if (state.Marker is >= 0xD0 and <= 0xD7)
        {
            if (!state.Scan) throw new ImageDataException("JPEG restart marker outside a scan.");
            goto NextMarker;
        }
        state.Scan = false;
        if (state.Marker == 0xD9)
        {
            if (!state.Frame || !state.ScanSeen) throw new ImageDataException("Invalid JPEG end.");
            if (!CompleteJpegOperation(r.EndAsync(), JpegResume.End, ref state, ref pending)) return false;
            goto End;
        }
        if (state.Marker is 0 or 0xD8 or 1) throw new ImageDataException("Invalid JPEG marker sequence.");
        if (!CompleteJpegOperation(r.ReadAsync(2), JpegResume.Length, ref state, ref pending)) return false;
    Length:
        var length = BinaryPrimitives.ReadUInt16BigEndian(r.Header);
        if (length < 2) throw new ImageDataException("Invalid JPEG segment length.");
        state.Size = length - 2; state.End = r.Position + state.Size; r.DeclaredEnd(state.End);
        if (state.Marker is >= 0xC0 and <= 0xCF && state.Marker is not (0xC4 or 0xC8 or 0xCC))
        {
            if (state.Marker is not (0xC0 or 0xC2)) throw new UnsupportedImageException(UnsupportedImageFeature.CodingProfile, "This JPEG coding profile is not supported.", ImageFormat.Jpeg);
            if (state.Frame || state.Size < 6) throw new ImageDataException("Invalid JPEG frame header.");
            if (!CompleteJpegOperation(r.ReadAsync(6), JpegResume.Frame, ref state, ref pending)) return false;
            goto Frame;
        }
        if (state.Marker == 0xDA)
        {
            if (!state.Frame || state.Size < 4) throw new ImageDataException("Invalid JPEG scan header.");
            if (!CompleteJpegOperation(r.ReadAsync(1), JpegResume.Scan, ref state, ref pending)) return false;
            goto Scan;
        }
        if (state.Marker is >= 0xE0 and <= 0xEF || state.Marker == 0xFE)
        {
            Metadata(r, state.Size, ref state.Metadata);
            if (state.Marker == 0xE0 && state.Size >= 5 || state.Marker == 0xEE && state.Size >= 12)
            {
                state.Count = state.Marker == 0xE0 ? 5 : 12;
                if (!CompleteJpegOperation(r.ReadAsync(state.Count), JpegResume.Application, ref state, ref pending)) return false;
                goto Application;
            }
            if (state.Marker == 0xE1 && state.Size >= 6 && (probe is null || probe.WantsOrientation))
            {
                if (!CompleteJpegOperation(r.ReadAsync(6), JpegResume.ExifPrefix, ref state, ref pending)) return false;
                goto ExifPrefix;
            }
        }
        goto Skip;

    Frame:
        if (r.Header[0] != 8) throw new UnsupportedImageException(UnsupportedImageFeature.BitDepth, "Only 8-bit JPEG is supported.", ImageFormat.Jpeg);
        state.Height = BinaryPrimitives.ReadUInt16BigEndian(r.Header.AsSpan(1)); state.Width = BinaryPrimitives.ReadUInt16BigEndian(r.Header.AsSpan(3)); state.Count = r.Header[5];
        if (state.Count is not (1 or 3 or 4)) throw new UnsupportedImageException(UnsupportedImageFeature.ComponentLayout, "JPEG component layout is not supported.", ImageFormat.Jpeg);
        if (state.Size != 6 + state.Count * 3) throw new ImageDataException("Invalid JPEG frame component count.");
        Dimensions(r, state.Width, state.Height); state.Frame = true;
        if (!CompleteJpegOperation(r.ReadAsync(state.Count * 3), JpegResume.Components, ref state, ref pending)) return false;
    Components:
        state.Source.Frame(state.Count, r.Header);
        probe?.Animation(false);
        if (probe?.Reached(ImageProbeFacts.Dimensions | ImageProbeFacts.Transparency
            | (state.Count == 1 ? ImageProbeFacts.SourceRepresentation : 0)) == true) return true;
        goto Skip;

    Scan:
        if (r.Header[0] is 0 or > 4 || state.Size != 4 + 2 * r.Header[0]) throw new ImageDataException("Invalid JPEG scan component count.");
        state.Scan = state.ScanSeen = true;
        goto Skip;

    Application:
        state.Source.Application(state.Marker, r.Header.AsSpan(0, state.Count));
        goto Skip;

    ExifPrefix:
        if (!r.Header.AsSpan(0, 6).SequenceEqual("Exif\0\0"u8)) goto Skip;
        if (state.Exif && metadataPolicy == ImageMetadataPolicy.Strict) throw new ImageDataException("Duplicate JPEG EXIF.");
        var exif = ExifAsync(r, state.Size - 6, false, metadataPolicy);
        if (!exif.IsCompletedSuccessfully)
        {
            state.Resume = JpegResume.Exif; pending = new JpegPending(exif); return false;
        }
        state.Orientation = exif.Result ?? state.Orientation;
    Exif:
        state.Exif = true;
        if (metadataPolicy == ImageMetadataPolicy.Strict) probe?.Orientation();
        if (state.Frame && probe?.Reached(0) == true) return true;
        goto Skip;

    Skip:
        if (!CompleteJpegOperation(r.SkipAsync(state.End - r.Position), JpegResume.Start, ref state, ref pending)) return false;
        goto NextMarker;

    End:
        probe?.Animation(false);
        if (probe?.WantsOrientation == true) probe.Orientation();
        probe?.Reached(ImageProbeFacts.All);
        return true;
    }

    private static async ValueTask<TResult> WebpAsync<TResult, TFactory>(InspectionReader r, long end, ProbeReader? probe, ImageMetadataPolicy metadataPolicy)
        where TResult : struct where TFactory : struct, IInspectionResult<TResult>
    {
        if (end < 12) throw new ImageDataException("Invalid WebP RIFF length."); r.DeclaredEnd(end);
        int items = 0, width = 0, height = 0, canvasW = 0, canvasH = 0; long metadata = 0;
        bool extended = false, pixels = false, alpha = false, alphaChunk = false, lossless = false, exif = false, icc = false, xmp = false;
        byte flags = 0; var orientation = ImageOrientation.Normal;
        while (r.Position < end)
        {
            Item(r, ref items);
            if (end - r.Position < 8) throw new ImageDataException("Truncated WebP chunk.");
            await r.ReadAsync(8).ConfigureAwait(false); var type = Four(r.Header); var size = BinaryPrimitives.ReadUInt32LittleEndian(r.Header.AsSpan(4));
            var payloadEnd = r.Position + size;
            if (size + (long)(size & 1) > end - r.Position) throw new ImageDataException("Invalid WebP chunk length.");
            if (type == Four("VP8X"u8))
            {
                if (items != 1 || size < 10) throw new ImageDataException("Invalid WebP VP8X.");
                await r.ReadAsync(10).ConfigureAwait(false); flags = r.Header[0];
                if ((flags & 2) != 0 && probe is null) throw new UnsupportedImageException(UnsupportedImageFeature.Animation, "Animated WebP is not supported.", ImageFormat.WebP);
                canvasW = U24(r.Header.AsSpan(4)) + 1; canvasH = U24(r.Header.AsSpan(7)) + 1;
                if ((long)canvasW * canvasH > uint.MaxValue) throw new ImageDataException("Invalid WebP canvas pixel count.");
                Dimensions(r, canvasW, canvasH); extended = true; alpha = (flags & 16) != 0;
                if (probe is not null)
                {
                    probe.Animation((flags & 2) != 0);
                    if ((flags & 8) == 0) probe.Orientation();
                    // VP8X settles the alpha hint, not whether a separate ALPH or VP8L channel follows.
                    if (probe.Reached(ImageProbeFacts.Dimensions))
                        return ImageSourceMetadata.Webp<TResult, TFactory>(probe, canvasW, canvasH, alpha, false, false, orientation);
                }
            }
            else if (type == Four("ANIM"u8) || type == Four("ANMF"u8))
                throw new UnsupportedImageException(UnsupportedImageFeature.Animation, "Animated WebP is not supported.", ImageFormat.WebP);
            else if (type == Four("VP8L"u8))
            {
                if (pixels || alphaChunk || size < 5) throw new ImageDataException("Invalid WebP VP8L header.");
                await r.ReadAsync(5).ConfigureAwait(false);
                if (r.Header[0] != 0x2F) throw new ImageDataException("Invalid WebP VP8L signature.");
                var bits = BinaryPrimitives.ReadUInt32LittleEndian(r.Header.AsSpan(1));
                if (bits >> 29 != 0) throw new UnsupportedImageException(UnsupportedImageFeature.Version, "Unsupported VP8L version.", ImageFormat.WebP);
                width = (int)(bits & 0x3FFF) + 1; height = (int)((bits >> 14) & 0x3FFF) + 1;
                alpha |= (bits & (1 << 28)) != 0; pixels = lossless = true;
            }
            else if (type == Four("VP8 "u8))
            {
                if (pixels || size < 10) throw new ImageDataException("Invalid WebP VP8 header.");
                await r.ReadAsync(10).ConfigureAwait(false);
                if ((r.Header[0] & 1) != 0 || !r.Header.AsSpan(3, 3).SequenceEqual(new byte[] { 0x9D, 1, 0x2A })) throw new ImageDataException("Invalid WebP VP8 keyframe.");
                width = BinaryPrimitives.ReadUInt16LittleEndian(r.Header.AsSpan(6)) & 0x3FFF; height = BinaryPrimitives.ReadUInt16LittleEndian(r.Header.AsSpan(8)) & 0x3FFF;
                if (width == 0 || height == 0) throw new ImageDataException("Invalid VP8 dimensions."); pixels = true;
            }
            else if (type == Four("ALPH"u8))
            {
                if (!extended || pixels || alphaChunk || size == 0 || !alpha) throw new ImageDataException("Invalid WebP alpha chunk."); alphaChunk = true;
            }
            else
            {
                Metadata(r, size, ref metadata);
                if (type == Four("EXIF"u8) && (probe is null || probe.WantsOrientation))
                {
                    if (!extended || exif && metadataPolicy == ImageMetadataPolicy.Strict || (flags & 8) == 0) throw new ImageDataException("Invalid WebP EXIF chunk.");
                    orientation = (await ExifAsync(r, size, true, metadataPolicy).ConfigureAwait(false)) ?? orientation; exif = true;
                    if (metadataPolicy == ImageMetadataPolicy.Strict) probe?.Orientation();
                    if (pixels && probe?.Reached(0) == true) return ImageSourceMetadata.Webp<TResult, TFactory>(probe, width, height, alpha, lossless, alphaChunk, orientation);
                }
                else if (type == Four("ICCP"u8))
                {
                    if (!extended || icc || pixels || (flags & 32) == 0) throw new ImageDataException("Invalid WebP ICC chunk."); icc = true;
                }
                else if (type == Four("XMP "u8))
                {
                    if (!extended || xmp || (flags & 4) == 0) throw new ImageDataException("Invalid WebP XMP chunk."); xmp = true;
                }
            }
            if (probe is not null && pixels)
            {
                if (extended && (width != canvasW || height != canvasH)) throw new ImageDataException("Invalid WebP canvas geometry.");
                Dimensions(r, width, height);
                if (!extended) { probe.Animation(false); probe.Orientation(); }
                if (probe.Reached(ImageProbeFacts.Dimensions | ImageProbeFacts.SourceRepresentation | ImageProbeFacts.Transparency))
                    return ImageSourceMetadata.Webp<TResult, TFactory>(probe, width, height, alpha, lossless, alphaChunk, orientation);
            }
            await r.SkipAsync(payloadEnd - r.Position).ConfigureAwait(false);
            if ((size & 1) != 0) { if (await r.ByteAsync().ConfigureAwait(false) != 0) throw new ImageDataException("Invalid WebP padding."); }
        }
        if (!pixels || extended && (width != canvasW || height != canvasH)) throw new ImageDataException("Invalid WebP canvas geometry.");
        if (extended && (((flags & 8) != 0) != exif || ((flags & 32) != 0) != icc || ((flags & 4) != 0) != xmp || !lossless && alpha != alphaChunk)) throw new ImageDataException("WebP feature flags disagree with chunks.");
        Dimensions(r, width, height); await r.EndAsync().ConfigureAwait(false);
        if (probe?.WantsOrientation == true) probe.Orientation();
        probe?.Animation(false); probe?.Reached(ImageProbeFacts.All);
        return ImageSourceMetadata.Webp<TResult, TFactory>(probe, width, height, alpha, lossless, alphaChunk, orientation);
    }

    private static async ValueTask<ImageOrientation?> ExifAsync(InspectionReader r, long length, bool optionalPrefix,
        ImageMetadataPolicy metadataPolicy)
    {
        var end = r.Position + length;
        try { return await ParseExifAsync(r, length, optionalPrefix, metadataPolicy).ConfigureAwait(false); }
        catch (InvalidExifException exception)
        {
            if (metadataPolicy == ImageMetadataPolicy.Strict) throw new ImageDataException(exception.Message, exception);
            // Only EXIF semantic errors are optional. Reads/seeks still prove the
            // outer payload exists; their truncation, limit and cancellation failures escape.
            await r.SkipAsync(end - r.Position).ConfigureAwait(false);
            return null;
        }
    }

    private static async ValueTask<ImageOrientation?> ParseExifAsync(InspectionReader r, long length, bool optionalPrefix,
        ImageMetadataPolicy metadataPolicy)
    {
        var end = r.Position + length;
        if (length < 8) throw new InvalidExifException("Truncated EXIF header.");
        await r.ReadAsync(8).ConfigureAwait(false);
        bool little; uint offset;
        if (optionalPrefix && r.Header.AsSpan(0, 6).SequenceEqual("Exif\0\0"u8))
        {
            if (length < 14) throw new InvalidExifException("Truncated EXIF header.");
            little = ByteOrder(r.Header.AsSpan(6, 2));
            await r.ReadAsync(6).ConfigureAwait(false);
            if (U16(r.Header, little) != 42) throw new InvalidExifException("Invalid TIFF identifier.");
            offset = U32(r.Header.AsSpan(2), little);
        }
        else
        {
            little = ByteOrder(r.Header.AsSpan(0, 2));
            if (U16(r.Header.AsSpan(2), little) != 42) throw new InvalidExifException("Invalid TIFF identifier.");
            offset = U32(r.Header.AsSpan(4), little);
        }
        if (offset < 8 || offset - 8L > end - r.Position - 2) throw new InvalidExifException("Invalid EXIF directory offset.");
        await r.SkipAsync(offset - 8L).ConfigureAwait(false); await r.ReadAsync(2).ConfigureAwait(false);
        var count = U16(r.Header, little);
        var truncated = count * 12L + 4 > end - r.Position;
        if (truncated && metadataPolicy == ImageMetadataPolicy.Strict) throw new InvalidExifException("Truncated EXIF directory.");
        if (count > r.Context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "EXIF directory entry count limit exceeded.");
        if (truncated) throw new InvalidExifException("Truncated EXIF directory.");
        var found = false; ImageOrientation? orientation = null;
        for (var i = 0; i < count; i++)
        {
            r.Context.Cancellation.ThrowIfCancellationRequested(); await r.ReadAsync(12).ConfigureAwait(false);
            if (U16(r.Header, little) != 0x0112) continue;
            if (found && metadataPolicy == ImageMetadataPolicy.Strict) throw new InvalidExifException("Duplicate EXIF orientation field.");
            if (U16(r.Header.AsSpan(2), little) != 3 || U32(r.Header.AsSpan(4), little) != 1)
            {
                if (metadataPolicy == ImageMetadataPolicy.Strict) throw new InvalidExifException("Invalid EXIF orientation field.");
                continue;
            }
            var value = U16(r.Header.AsSpan(8), little);
            if (value is < 1 or > 8)
            {
                if (metadataPolicy == ImageMetadataPolicy.Strict) throw new InvalidExifException("Invalid EXIF orientation value.");
                continue;
            }
            orientation = (ImageOrientation)value; found = true;
        }
        await r.SkipAsync(end - r.Position).ConfigureAwait(false);
        return orientation;
    }

    private static bool ByteOrder(ReadOnlySpan<byte> bytes)
        => bytes.SequenceEqual("II"u8) ? true : bytes.SequenceEqual("MM"u8) ? false : throw new InvalidExifException("Invalid EXIF byte order.");

    private sealed class InvalidExifException(string message) : Exception(message);
}
