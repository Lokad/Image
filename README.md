# Lokad.Image

A safe C#/.NET 10 library for still-image codecs and basic editing. Independently
implemented, with no NuGet dependencies, unsafe code or native bindings.

The 0.2.0 development API defaults to the JPEG `Reference` profile for baseline
grayscale and 4:4:4/4:2:2/4:2:0. [Codec compatibility](docs/codec-parity.md) states its exact tested scope.

- **PNG:** decode all static color/depth profiles, including palette, transparency
  and Adam7; encode grayscale/RGB/grayscale-alpha/RGBA at 8 or 16 bits.
- **JPEG:** baseline/progressive decode; baseline encode with grayscale,
  subsampling, restart markers and explicit alpha flattening.
  CMYK/YCCK and 4:1:1/4:1:0 decode; caller-owned gray/RGB/CMYK decode samples
  and gray/RGB encode samples. Explicit legacy profiles retain admitted progressive
  4:4:4 encoding and optional compact color quantization.
- **WebP:** lossy VP8 and lossless VP8L, including alpha and lossless hidden RGB.
- **Editing:** 8/16-bit RGBA, EXIF orientation, crop, flips, quarter turns,
  transpose and nearest/bilinear/bicubic/Lanczos3 resize; rectangle fill/copy and alpha compositing.
- **Streams:** synchronous/asynchronous identification, decoding and encoding;
  fixed-workspace dimension/orientation inspection with `InspectHeaders`.
- **Text (in development):** supplied TrueType faces, wrapping, alignment, pixel
  bounds and rendering on 8/16-bit canvases. Explicit OpenType mode adds Latin,
  Hebrew and Arabic shaping with bidirectional layout and constrained-area fitting.

GIF, animation, ICC color management and metadata round-tripping are deferred.
[SPEC.md](SPEC.md) defines current support and planned still-image extensions,
including supplied-font text.

```powershell
dotnet add package Lokad.Image --version 0.1.0
```

```csharp
using Lokad.Image;

using var input = File.OpenRead("input.png");
using var image = ImageCodec.Decode(input, new DecodeOptions { ApplyOrientation = true });
using var thumbnail = ImageTransforms.ResizeToFit(image, 320, 240, allowUpscaling: false);
using var output = File.Create("thumbnail.png");
ImageCodec.EncodePng(thumbnail, output);
```

Dispose image buffers, fonts and prepared text layouts; supplied streams remain open. Operations support finite
[resource limits](docs/resource-admission.md) and cancellation. PNG preserves
precision; JPEG/WebP output is 8-bit. WebP defaults to lossless. For transparent
JPEG input, set `JpegEncodeOptions.BackgroundRgb` explicitly.
Development toward 0.2.0 adds caller-owned cooperative memory admission through
`ImageLimits.MemoryBudget`. `ImageBudget` separately provides codec memory/work quotas.
`ImageInfo` reports encoded color, alpha representation and sample/index depth;
`InspectHeadersAsync` reads these with bounded workspace, without decoding pixels.

The [usage guide](docs/usage.md) covers encoding, cropping and 16-bit editing.
The published 0.1.0 scope met its ≤2× warm single-CPU target. Current
[benchmark results](docs/benchmark.md) state the tested scope, allocations,
compressed sizes and remaining 0.2.0 parity gaps.

For development, install the SDK in `global.json`, restore the solution, then run:

```powershell
./test.ps1
./test.ps1 -Differential  # FFmpeg on PATH or LOKAD_IMAGE_FFMPEG
./tools/PackageSmokeTest/verify-package.ps1
```

Use [tools/setup-linux.sh](tools/setup-linux.sh) for the pinned Linux toolchain and
[benchmark instructions](benchmarks/README.md) for measurement. External tools
stay in ignored `external/`; performance measurements require the quiet VM.

Licensed under [MIT](LICENSE.txt).
