# Using Lokad.Image

These examples target .NET 10. See [SPEC.md](spec.md) for supported codec
profiles and the exact pixel, metadata and failure contracts.

## Decode, crop and resize

`Decode` recognizes PNG, JPEG and WebP from their contents. Apply EXIF orientation
before choosing a crop in displayed coordinates. Each transform returns a new
owned buffer; the source stays unchanged.

```csharp
using Lokad.Image;

using var input = File.OpenRead("input.png");
using var image = ImageCodec.Decode(input, new DecodeOptions { ApplyOrientation = true });
using var crop = ImageTransforms.CropCentered(image,
    Math.Min(256, image.Width), Math.Min(256, image.Height));
using var thumbnail = ImageTransforms.ResizeToFit(crop, 128, 96,
    allowUpscaling: false, filter: ResizeFilter.Lanczos3);
using var output = File.Create("thumbnail.png");
ImageCodec.EncodePng(thumbnail, output);
```

`Crop` takes a contained rectangle: `x`, `y`, `width`, `height`. `CropClamped`
moves a requested rectangle inside the source; its width and height must already
fit. `ResizeToFit` preserves aspect ratio. Choose `ResizeFilter.Nearest` when
individual samples must be preserved. Bilinear, bicubic and Lanczos3 resizing filter
straight RGBA through premultiplied color, then return straight RGBA.

## Probe dimensions before downloading pixels

Development 0.2.1 provides selective `Probe`/`ProbeAsync` on seekable and nonseekable
streams. The default reads dimensions before pixel payloads. The stream stays open
at the acquired prefix. Unexamined orientation/alpha/animation
fields are null. Select additional `ImageProbeFacts` when needed; late metadata
may require more input. Probing starts at the current position and does not rewind.
Dimension-only probing stops before pixel payloads; seeking JPEG entropy for late
metadata can read ahead by up to 8 KiB. Reopen or explicitly replay a consumed prefix
before decoding a nonseekable input. Use Identify/InspectHeaders for their stronger
container checks, ValidatePixels for pixel-coding validation, and Decode when pixels
are needed. See [source metadata](source-metadata.md).

## Work with 16-bit pixels

Rows contain consecutive red, green, blue and alpha samples. An 8-bit buffer uses
`GetRow8`; a 16-bit buffer uses `GetRow16`. The latter exposes unsigned samples
in native byte order, not an encoded file's byte order. A pixel starts at `x * 4`.
New buffers contain zero samples, including zero alpha.

```csharp
using Lokad.Image;

using var source = ImageBuffer.Create(2, 2, 16);
new ushort[] { 1000, 2000, 3000, 65535, 4000, 5000, 6000, 65535 }
    .CopyTo(source.GetRow16(0));
new ushort[] { 12345, 23456, 34567, 32768, 65535, 0, 65535, 0 }
    .CopyTo(source.GetRow16(1));

using var crop = ImageTransforms.Crop(source, 0, 0, 1, 2);
using var flipped = ImageTransforms.FlipVertical(crop);
using var rotated = ImageTransforms.Rotate(flipped, QuarterTurn.Clockwise90);
using var enlarged = ImageTransforms.Resize(rotated, 4, 2, ResizeFilter.Nearest);
using var output = File.Create("samples16.png");
ImageCodec.EncodePng(enlarged, output);
```

Crop, flips, quarter turns and nearest resizing retain the source precision and
copy samples exactly, including RGB behind zero alpha. Filtered resizing clears
RGB when its rounded output alpha is zero. Resizing to the current dimensions
copies the samples without filtering. PNG encoding preserves 8/16-bit source
precision. JPEG and WebP encoding reduce 16-bit input to 8-bit once. JPEG requires
an explicit `BackgroundRgb` for transparent input; flattening and depth reduction
share that rounding. WebP encoding defaults to lossless; set
`WebpEncodeOptions.Lossless = false` explicitly for lossy encoding.

## Share memory admission and use asynchronous streams

The caller supplies its `IMemoryBudget` root and owns both streams. Forward that
root and the current token through every stage; local limits still apply to each
operation. This function accepts an input positioned at the image beginning and
writes directly to the supplied destination.

```csharp
using Lokad.Image;

static async Task WriteThumbnailAsync(Stream input, Stream output,
    IMemoryBudget memoryBudget, ImageFormat format, CancellationToken token)
{
    var limits = new ImageLimits
    {
        MemoryBudget = memoryBudget,
        MaxDimension = 1024, MaxPixels = 1_000_000,
        MaxInputBytes = 1024 * 1024, MaxOutputBytes = 4 * 1024 * 1024,
        MaxWorkingBytes = 64L * 1024 * 1024,
    };
    using var image = await ImageCodec.DecodeAsync(input,
        new DecodeOptions { ApplyOrientation = true, Limits = limits }, token);
    using var thumbnail = ImageTransforms.ResizeToFit(image, 128, 96,
        allowUpscaling: false, limits: limits, cancellationToken: token);
    switch (format)
    {
        case ImageFormat.Png:
            await ImageCodec.EncodePngAsync(thumbnail, output, new() { Limits = limits }, token);
            break;
        case ImageFormat.Jpeg:
            await ImageCodec.EncodeJpegAsync(thumbnail, output,
                new() { Limits = limits, Quality = 85, Sampling = JpegSampling.YCbCr420, BackgroundRgb = 0xffffff }, token);
            break;
        case ImageFormat.WebP:
            await ImageCodec.EncodeWebpAsync(thumbnail, output,
                new() { Limits = limits, Lossless = true, Effort = 4 }, token);
            break;
        default: throw new ArgumentOutOfRangeException(nameof(format));
    }
}
```

Images stay charged until disposal, including their overlap during resizing.
Input acquisition and encoder staging remain charged until actual async users
finish; requesting cancellation does not end a borrow. Streams and the root stay
open. `MaxWorkingBytes` counts accounted source/result/scratch capacity, not RSS;
shared admission charges owned storage rather than charging borrowed inputs again.

If the destination retains encoded bytes, the caller must admit its capacity and
keep that storage owner alive through publication. Codec completion does not fund
arbitrary surviving copies. A failed write can leave partial output; publication
or atomic file replacement is the caller's policy. See [resource admission](resource-admission.md).

## Ownership, inspection and failures

Dispose every `ImageBuffer` you own. Row spans refer to that buffer's mutable
storage and should only be used while it is alive. Supplied streams stay open;
callers own their positions and disposal.
Failure or cancellation leaves the position reached by the underlying I/O;
there is no rewind or rollback. Successful full read routes finish at EOF.

`ImageCodec.Identify` reports coded dimensions, precision and recognized EXIF
orientation without constructing a pixel buffer. `ImageInfo.OrientedWidth` and
`OrientedHeight` describe the displayed dimensions. Identification does not
validate the complete compressed pixel payload. Decode performs that work.

Use `ImageCodec.ValidatePixels(encodedBytes)` or
`await ImageCodec.ValidatePixelsAsync(input, limits, token)` when pixel-coding
validity is needed without a retained raster. Results describe coded dimensions
and pending orientation. These operations use codec-dependent memory, read streams
through EOF and leave them open. Pass `metadataPolicy: ImageMetadataPolicy.IgnoreInvalid`
explicitly when needed. PNG retains compressed input/rows/inflater, JPEG retains
coefficients and WebP retains planes/history; validation is not fixed-memory or
an incremental decoder. Use Decode directly when pixels are needed.

`Decode(Stream)` reads through EOF into private storage and decodes its acquired
prefix without a final encoded-input copy. Returned pixels own separate storage.
Actual retained input capacity counts toward the working limit.

`IdentifyAsync(Stream)` and `DecodeAsync(Stream)` asynchronously acquire input
through EOF, then run the same CPU validation/decoding. Both accept cancellation,
leave input open. Keep the stream
alive until completion and dispose returned image owners normally. Async input
uses bounded encoded-input buffering.

`InspectHeaders(Stream)` and `InspectHeadersAsync(Stream)` obtain dimensions and
orientation with fixed workspace. They scan through
EOF, seeking over opaque payloads when possible and otherwise draining them.
PNG CRCs and compressed pixels are not validated; `Identify` retains its fuller
container checks. Configured limits and cancellation still apply. Input stays open.

`EncodePngAsync`, `EncodeJpegAsync` and lossless `EncodeWebpAsync` stage encoded
bytes within working/output limits; lossy WebP delivers prepared partitions.
All write asynchronously, leave output open and
use the same memory-admission root. The JPEG sample overload accepts borrowed
`ReadOnlyMemory<byte>` gray/RGB samples with the same stride/quality controls.
Keep source storage stable until completion. No final `ToArray` copy is needed;
discard partial output after a write failure or cancellation.

For codec embedding, `DecodeJpeg(encoded.Span, destination.Span, format, stride,
options, token)` writes caller-owned grayscale/RGB/CMYK8 samples. Choose
`JpegDecodeOptions.SourceColorSpace`, `CmykInversion` and `ApplyOrientation`
explicitly; ICC conversion is not applied. Destination padding stays untouched
on success, while failure can leave partial pixels. Its geometry must fit the
chosen orientation. `EncodeJpegAsync(samples, width, height, format, output,
stride, options, token)` borrows grayscale/RGB8 samples without an RGBA frame.
Keep the sample owner alive and stable through completion. Caller sample storage
is separate from codec scratch; local working limits include the declared spans,
while shared admission does not charge those borrowed buffers again.

`ImageDrawing.FillRectangle`, `CopyRegion` and `Composite` mutate a canvas in coded
coordinates with clipped destinations. Copy source rectangles must fit and depths
must match; compositing accepts either source/canvas depth. `RgbaColor.From8` expands
eight-bit colors exactly. Self-overlap is safe without a full-frame temporary.
Mutations can leave completed chunks on cancellation; synchronize
access to mutable images. No EXIF orientation is implicitly applied.

`TrueTypeFont.Load` owns a supplied standalone TrueType face. Dispose fonts and
`TextLayout` instances after their last user. Prepared outlines remain valid
after font disposal. Create immutable
pixel layout with `TextLayout.Create(font, text, new TextOptions { FontSize = 16,
WrapWidth = 240, LineSpacing = 1.2 })`. `LayoutBounds`, `InkBounds` and their union
`Bounds` support measurement; use `Bounds.Translate(x, y).FitsInside(width, height)`
to check positioned overflow. Render with `ImageDrawing.DrawText(canvas, layout,
x, y, RgbaColor.From8(20, 40, 60))`; destinations clip and preserve canvas precision.
Styles come from supplied faces. Add `Shaping = TextShaping.OpenType` for supported
Latin/Hebrew/Arabic shaping and bidirectional layout; `Direction` selects paragraph
direction and `Alignment` controls line alignment. The default selects logical left-to-right layout. [The contract](spec.md#supplied-fonts-and-text) defines supported font
tables, wrapping and admission.

For a constrained area, call `TextLayout.TryFit(font, text, 240, 80, out var fitted,
new TextOptions { FontSize = 24, Shaping = TextShaping.OpenType },
minimumFontSize: 8, fontSizeStep: .25)`. On success, dispose `fitted` after use
and draw it at `(-fitted.Bounds.X, -fitted.Bounds.Y)` relative to the target area.
The largest fitting grid candidate is returned; false/null means none fits.
Trials share admission and release rejected storage. Resource failure or
cancellation terminates the call rather than trying a smaller candidate.

For cooperative memory admission, set
`new ImageLimits { MemoryBudget = callerAdapter }` and pass these limits through
codec/text options and editing calls. The adapter implements Lokad.Image's
`IMemoryBudget`/`IMemoryScope`/`IMemoryReservation` interfaces against the caller's
shared allowance. Keep it alive through all operations and retained objects;
dispose only the scopes the caller owns. Results release their own charges on
disposal. [Resource admission](resource-admission.md) defines borrowing, refusal,
capacity estimates and asynchronous lifetimes.

Malformed input raises `ImageDataException`, unsupported formats or profiles
raise `UnsupportedImageException`, exhausted admission limits raise
`ImageLimitException`, and cancellation raises `OperationCanceledException`.
Use `ImageLimitException.Scope` and `Limit`, and `UnsupportedImageException.Feature`
for structured classification. `SharedBudget` refusal is terminal, with no retry,
wait or lower-quality fallback; it is not corrupt coding. `IgnoreInvalid` tolerates
optional metadata only and preserves required coding/container and resource checks.
Invalid API arguments use standard argument exceptions. Stream I/O failures
propagate. Encoders strip optional metadata; ICC color management and metadata
round-tripping are deferred.
