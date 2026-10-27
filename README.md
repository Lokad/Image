# Lokad.Image

Safe, dependency-free C# image codecs and editing for **.NET 10**.
No unsafe code, native bindings, subprocess codecs or vendored sources.

- PNG, JPEG and WebP encode/decode, including progressive/CMYK/YCCK JPEG decode.
- 8/16-bit RGBA editing: crop, orientation, flips, rotations, nearest/bilinear/
  bicubic/Lanczos3 resizing, drawing and alpha compositing.
- Supplied TrueType text measurement, wrapping, alignment, fitting and rendering;
  explicit OpenType mode for supported Latin, Hebrew and Arabic shaping.
- Caller JPEG sample buffers, async streams, bounded header inspection, typed
  source metadata and caller-owned cooperative memory admission.

```powershell
dotnet add package Lokad.Image --version 0.2.0
```

```csharp
using Lokad.Image;

using var input = File.OpenRead("input.png");
using var image = ImageCodec.Decode(input, new DecodeOptions { ApplyOrientation = true });
using var thumbnail = ImageTransforms.ResizeToFit(image, 320, 240, allowUpscaling: false);
using var output = File.Create("thumbnail.png");
ImageCodec.EncodePng(thumbnail, output);
```

Dispose images, fonts and layouts after their final users; caller streams stay open.
Operations support finite [resource limits and shared admission](docs/resource-admission.md)
and cooperative cancellation. PNG retains 8/16-bit precision; JPEG/WebP encode at
8 bits. WebP defaults to lossless. Transparent JPEG input needs explicit
`JpegEncodeOptions.BackgroundRgb`.

GIF, animation, ICC conversion and metadata round-tripping are deferred. Font
support has explicit table/script boundaries. See the [supported contract](docs/spec.md),
[usage](docs/usage.md) and [0.2.0 migration notes](docs/migration.md).
The [codec parity table](docs/codec-parity.md) and [benchmark snapshot](docs/benchmark.md)
state tested scope; full CPU parity and general replacement readiness remain open.

For development, install the SDK in `global.json`, restore, then run:

```powershell
./test.ps1
./test.ps1 -Differential  # requires FFmpeg
./tools/PackageSmokeTest/verify-package.ps1
```

[Linux setup](tools/setup-linux.sh) · [Benchmark instructions](benchmarks/README.md) ·
[MIT license](LICENSE.txt)
