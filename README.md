# Lokad.Image

Lokad.Image is a safe C#/.NET 10 image library under development. The production
NuGet package has no dependencies. All codec and editing implementations are
owned code; FFmpeg is a development oracle and benchmark baseline.

Current functionality includes PNG decoding/encoding, baseline JPEG
decoding/encoding, progressive JPEG decoding, lossless WebP decoding/encoding,
lossy WebP decoding/encoding with exact alpha, 8/16-bit RGBA buffers, crop, resize,
flips, quarter turns and EXIF orientation. Encoders prioritize correctness;
compression efficiency and further performance tuning remain ahead. The initial
[dedicated-VM invocation results](benchmarks/RESULTS.md) document the measured
workloads, encoded sizes, quality exclusions and memory scope.
See [SPEC.md](SPEC.md) for the precise support matrix.
The [usage guide](docs/usage.md) covers cropping, 16-bit editing, limits,
cancellation and buffer ownership with runnable examples.
GIF, animation, drawing and font rendering are outside the initial scope.

```csharp
using Lokad.Image;

using var input = File.OpenRead("input.png");
using var image = ImageCodec.Decode(input, new DecodeOptions { ApplyOrientation = true });
using var thumbnail = ImageTransforms.ResizeToFit(image, 320, 240, allowUpscaling: false);
using var output = File.Create("thumbnail.png");
ImageCodec.EncodePng(thumbnail, output);
```

To write the same thumbnail as JPEG or lossy WebP:

```csharp
using var jpegOutput = File.Create("thumbnail.jpg");
ImageCodec.EncodeJpeg(thumbnail, jpegOutput,
    new JpegEncodeOptions { Quality = 85, BackgroundRgb = 0xFFFFFF });

using var webpOutput = File.Create("thumbnail.webp");
ImageCodec.EncodeWebp(thumbnail, webpOutput,
    new WebpEncodeOptions { Lossless = false, Quality = 85, Effort = 4 });
```

JPEG requires an explicit background for transparent input. JPEG and WebP output
uses 8-bit samples; 16-bit input is reduced once, with JPEG background flattening
included in that same rounding. PNG preserves the selected source precision.

Callers own supplied streams, which remain open. Image buffers own their memory
and must be disposed. Operations accept finite `ImageLimits` and cancellation.
Stop using row spans before disposing their owner; released pixel storage may
be reused by later operations.
Crop, flips and half turns retain complete channel bytes at both sample depths, with
bounded work/cancellation checks and independently owned outputs.
The [resource admission notes](docs/resource-admission.md) explain what the limits
count and the current hardening evidence.
Full ICC color management and metadata round-tripping are not implemented.
WebP output defaults to lossless; use `new WebpEncodeOptions { Lossless = false }`
for lossy VP8. The original [quality/size reports](tests/Quality/README.md) record
JPEG and VP8 compression limitations and the FFmpeg comparison methods.

For development, install the SDK selected by `global.json` and run:

```powershell
./test.ps1
./test.ps1 -Differential       # requires FFmpeg on PATH or LOKAD_IMAGE_FFMPEG
./tools/PackageSmokeTest/verify-package.ps1
```

Follow [the benchmark instructions](benchmarks/README.md) to generate a current
quality calibration before running `./bench.ps1 -Smoke`. Smoke runs verify
execution and outputs; performance measurement requires the dedicated VM.

After building and testing Release, `./tools/PackageSmokeTest/verify-package.ps1
-NoBuild` packages the existing binaries and verifies them with an isolated
package-only consumer. Omit `-NoBuild` to rebuild before packaging.
To verify an existing release artifact without repacking it, use
`./tools/PackageSmokeTest/verify-package.ps1 -PackagePath <package.nupkg>`.

After a successful restore, `./test.ps1 -NoRestore` uses only cached dependencies.
Add `-SerialBuild` to disable parallel build workers and compiler reuse when the
development environment restricts worker processes.

The offline suite uses original frozen fixtures generated with FFmpeg. Sources,
recipes, hashes and oracle settings are recorded under `tests/Fixtures`. Tools and
reference source checkouts belong under the ignored `external/` directory.

On Ubuntu 24.04 x86_64, provision the development build prerequisites and use the
local bootstrap for the pinned SDK, PowerShell and source-built FFmpeg oracle:

```bash
sudo apt-get update
sudo apt-get install -y build-essential cmake pkg-config perl autoconf automake libtool xz-utils curl git python3 libicu74 libssl3t64 zlib1g ca-certificates
bash tools/setup-linux.sh --jobs=4
source external/linux-x64/environment.sh
dotnet restore Lokad.Image.slnx --locked-mode --tl:off
pwsh -NoLogo -NoProfile -File ./test.ps1 -Differential
pwsh -NoLogo -NoProfile -File ./tools/PackageSmokeTest/verify-package.ps1
```

The bootstrap checks archive hashes and source revisions from
`tools/external-sources.json`; all downloaded and built tools stay in `external/`.
Its `oracle-identity.json` records actual binary hashes, build configuration and
compiler/dependency versions. Source-built hashes depend on the build environment.
Use `--toolchain-only` for offline development without building the native oracle.

Performance measurement and tuning require a quiet, dedicated VM. No performance
claims are made from benchmark smoke runs. The project is licensed under MIT.
