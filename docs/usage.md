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
using var thumbnail = ImageTransforms.ResizeToFit(crop, 128, 96, allowUpscaling: false);
using var output = File.Create("thumbnail.png");
ImageCodec.EncodePng(thumbnail, output);
```

`Crop` takes a contained rectangle: `x`, `y`, `width`, `height`. `CropClamped`
moves a requested rectangle inside the source; its width and height must already
fit. `ResizeToFit` preserves aspect ratio. Choose `ResizeFilter.Nearest` when
individual samples must be preserved. Bilinear and bicubic resizing filter
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
    MaxWork = 100_000_000
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
the charges and work limits. A failed encode may leave partial bytes in its
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

Malformed input raises `ImageDataException`, unsupported formats or profiles
raise `UnsupportedImageException`, exhausted admission limits raise
`ImageLimitException`, and cancellation raises `OperationCanceledException`.
Invalid API arguments use standard argument exceptions. Stream I/O failures
propagate. Encoders strip optional metadata; ICC color management and metadata
round-tripping are deferred.
