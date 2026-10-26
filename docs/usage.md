# Using Lokad.Image

These examples target .NET 10. See [SPEC.md](../SPEC.md) for supported codec
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
share that rounding. WebP encoding defaults to lossless; the README demonstrates
the explicit lossy options.

## Bound an operation and pass cancellation

Limits apply separately to each operation. Pass the same chosen policy and
cancellation token to every stage of a workflow.

```csharp
using Lokad.Image;

var limits = new ImageLimits
{
    MaxDimension = 1024,
    MaxPixels = 1_000_000,
    MaxInputBytes = 1024 * 1024,
    MaxOutputBytes = 4 * 1024 * 1024,
    MaxWorkingBytes = 64L * 1024 * 1024,
};
using var cancellation = new CancellationTokenSource();
var token = cancellation.Token;

using var input = File.OpenRead("input.png");
using var image = ImageCodec.Decode(input,
    new DecodeOptions { ApplyOrientation = true, Limits = limits }, token);
using var thumbnail = ImageTransforms.ResizeToFit(image, 128, 96,
    allowUpscaling: false, limits: limits, cancellationToken: token);
using var encoded = new MemoryStream();
ImageCodec.EncodePng(thumbnail, encoded, new PngEncodeOptions { Limits = limits }, token);
File.WriteAllBytes("bounded-thumbnail.png", encoded.ToArray());
```

Connect the token to the caller's cancellation source. `MaxWorkingBytes` admits
the operation's accounted source, result and scratch buffers; it is not a cap
on process RSS. The [resource admission notes](resource-admission.md) define
the charges and local memory limits. A failed encode may leave partial bytes in its
output stream; discard them. The example writes the destination after encoding
succeeds.

## Ownership, inspection and failures

Dispose every `ImageBuffer` you own. Row spans refer to that buffer's mutable
storage and should only be used while it is alive. Supplied streams stay open;
callers own their positions and disposal.

`ImageCodec.Identify` reports coded dimensions, precision and recognized EXIF
orientation without constructing a pixel buffer. `ImageInfo.OrientedWidth` and
`OrientedHeight` describe the displayed dimensions. Identification does not
validate the complete compressed pixel payload. Decode performs that work.

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

`EncodePngAsync`, `EncodeJpegAsync` and `EncodeWebpAsync` stage encoded bytes within
working/output limits, then write asynchronously. They leave output open and
use the same memory-admission root. The JPEG sample overload accepts borrowed
`ReadOnlyMemory<byte>` gray/RGB samples with the same stride/quality controls.
Keep source storage stable until completion. No final `ToArray` copy is needed;
discard partial output after a write failure or cancellation.

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
direction and `Alignment` controls line alignment. The default selects logical left-to-right layout. [SPEC.md](../SPEC.md#supplied-truetype-fonts) defines supported font
tables, wrapping and admission.

For a constrained area, call `TextLayout.TryFit(font, text, 240, 80, out var fitted,
new TextOptions { FontSize = 24, Shaping = TextShaping.OpenType },
minimumFontSize: 8, fontSizeStep: .25)`. On success, dispose `fitted` after use
and draw it at `(-fitted.Bounds.X, -fitted.Bounds.Y)` relative to the target area.
The largest fitting grid candidate is returned; false/null means none fits.
Trials share admission and release rejected storage. Resource failure or
cancellation terminates the call rather than trying a smaller candidate.

For cooperative memory admission in 0.2.0 development, set
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
Invalid API arguments use standard argument exceptions. Stream I/O failures
propagate. Encoders strip optional metadata; ICC color management and metadata
round-tripping are deferred.
