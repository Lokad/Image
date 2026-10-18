# Original fixture corpus

TrueType controls are generated in tests by `tools/Shared/AuthoredTrueType.cs`:
original rectangles, triangles, a quadratic contour and a two-component accent,
with known advances, bounds and Unicode mappings. The table layout follows the
public [OpenType specification](https://learn.microsoft.com/en-us/typography/opentype/spec/otff).
No external font bytes or outlines are included. These controls do not change
the frozen image-corpus counts below.

`dotnet run -c Release --project tools/Fixtures -- <output> --font-layout`
generates three tiny original font controls and managed geometry records without
regenerating images: ordinary Unicode/quadratic outlines, packed BMP contours and
composite transforms/attachments. `tools/WarmReference/freetype-outline-check.c`
checks unhinted font-unit glyph bounds/advances through FreeType's public API,
using headers only from ignored `external/`. This is a development correctness
check, not a text-performance baseline. Font binaries remain generated/ignored.

These images originate from Lokad.Image's own deterministic `asymmetric-v1`
pixel recipe in `tools/Shared/OriginalPixels.cs` and independently chosen T.81
coefficient/scan cases in `tools/Shared/AuthoredJpeg.cs` and VP8L bitstreams in
`tools/Shared/AuthoredWebp.cs`. No upstream test images
are included. FFmpeg encodes PNG and baseline color JPEG source pixels and
independently decodes all expected RGBA samples. `manifest.json` and
`jpeg-manifest.json`, `webp-manifest.json`, `vp8-manifest.json` and
`vp8-authored-manifest.json` and `png-authored-manifest.json` record recipes,
argument vectors, color formats,
encoded/expected hashes and oracle identity.

The corpus contains 88 PNG, 30 JPEG, 60 lossless WebP and 164 lossy WebP fixtures.

The 42 managed VP8 fixtures retain their original sources and options. Their
`encoded-alpha-weighted-2x2-v1` provenance records alpha-weighted encoded-space
chroma preparation. Fifteen alpha bitstreams and their native decoded goldens
were refreshed for that conversion; opaque bitstreams and native fixtures remain
identical. With the manifest's registered original FFmpeg executable, use
`dotnet tools/Fixtures/bin/Release/net10.0/Fixtures.dll tests/Fixtures
--refresh-managed-vp8` to refresh only managed outputs. The command verifies
original input hashes and keeps all other fixture records and corpus counts.

Additional EXIF controls are generated during tests by `tools/Shared/OriginalExif.cs`;
they do not change these frozen corpus counts. The original wrapper writes one
IFD0 SHORT orientation tag in both TIFF byte orders, using the public
[Exif/TIFF layout](https://www.cipa.jp/std/documents/e/DC-X008-Translation-2019-E.pdf),
[PNG eXIf framing](https://www.w3.org/TR/png-3/#eXIf) and
[WebP extended-container framing](https://developers.google.com/speed/webp/docs/riff_container).
PNG8/PNG16, baseline/progressive 4:2:0 JPEG and lossless/lossy WebP cover all eight
values on asymmetric 9x7 rasters. The lossless sources contain partial alpha and
hidden RGB at an explicitly zero-alpha first pixel. Other than the original
progressive coefficient construction, the bitstreams use the owned encoders.
The independent reference places coded pixels forward by the declared row/column
directions and is checked against literal 3x2 grids at both sample depths.
Live native tests disable autorotation and apply the corresponding RGB filter
after color conversion; this verifies geometry rather than native EXIF discovery.

The 72 PNG constructions in `tools/FormatCases/AuthoredPng.cs` use independently
written packing, forward filtering, stored Deflate blocks, zlib framing and CRCs;
they share no production codec helpers. They cover all 15 legal color/depth
profiles, plain/Adam7 order, all five filters, tiny images with empty passes,
nonzero packed padding, split/empty IDAT chunks, partial palette transparency,
gray/RGB tRNS and 16-bit hidden RGB. Their descriptor and analytical sample hashes
are recorded alongside independently decoded FFmpeg output. Every authored profile
records `exact-rgba-samples`. The registered 8.1 decoder ignores tRNS alpha in
twelve transparent packed grayscale cases. Their recorded native pipeline applies
`format=rgba,colorkey=color=0xGGGGGG:similarity=0.01:blend=0` using the declared
transparent sample expanded to eight bits. Adjacent packed gray levels differ by
at least 17/255, so this native filter selects only that sample and preserves RGB.
The resulting `.rgba` is exact against the independent analytical samples;
`.direct.rgba` and its hash retain unfiltered decoder output for all twelve cases.
The schema-2 manifest records both pipelines and the observed decoder limitation.
Forty-four live controls cover every transparent key at both interlace settings.
Twelve opaque counterparts also have exact native RGBA.

Original fixed/dynamic Deflate prefix paths in `tools/FormatCases/AuthoredDeflate.cs`
provide separate analytical and live native checks, including a literal-only
dynamic alphabet and overlapping history. Malformed blocks, code repeats,
declared windows, checksum positions and missing final flags/trailers exercise
the production framing validator beyond chunk CRC failures.

JPEG cases include baseline
4:4:4/4:2:2/4:2:0 with odd and one-pixel dimensions, original managed baseline
output, and authored progressive streams with spectral selection, successive
approximation, positive/negative coefficients, zero/EOB runs and restart markers.
The pinned FFmpeg encoder does not emit progressive JPEG; those files are owned
binary constructions whose pixels are independently decoded by FFmpeg. Small
analytical expectations also validate the constructions in the public tests.

WebP includes FFmpeg output at two effort settings, original managed output at
four settings and both source depths, and independently authored streams covering
every predictor mode, signed color transforms, subtract-green, packed palettes,
spatial prefix groups, cache references, overlapping copies, code-length repeats,
duplicate simple symbols and EXIF before/after pixel data. Comparisons require
exact RGBA8 samples. FFmpeg may normalize hidden RGB under zero alpha when encoding;
expectations therefore come from its decoded bitstream. Managed encoding preserves
hidden RGB and is also checked against the normalized source samples.

`vp8-manifest.json` records lossy WebP cases covering three qualities, two efforts,
opaque/transparent asymmetric inputs, tiny/odd dimensions, uniform colors and
horizontal/vertical ramps. Each has exact planar YUV (and alpha when present) in
`.yuv`, plus independently converted RGBA in `.rgba`. Integer reconstruction and
alpha must match exactly; RGB conversion requires maximum error <=4 and RMSE <=1.5.
Chroma planes are expanded by two before cropping odd dimensions and merging,
then converted with explicit limited-range BT.601 settings. This separates
reconstruction correctness from scaler/matrix rounding differences.
Four independently authored raw ALPH wrappers exercise all alpha filters with
clipping and modulo cases; FFmpeg validates their decoded samples as well. Their
manifest records the base opaque encoder arguments, authoring method, alpha recipe
and source-alpha hash.
Another 42 fixtures contain owned VP8 encoder output at quality 1/100 and effort
0/6, plus quality 85/effort 4 with 16-bit sources. Their manifest records source
depth and managed options. FFmpeg independently supplies both exact YUV/alpha
and the bounded RGB expectations for these streams.

`vp8-authored-manifest.json` adds 54 original syntax cases. The builder in
`tools/FormatCases/AuthoredVp8.cs` uses an arbitrary-precision arithmetic interval,
and shares only specification-derived numeric constants with production code.
Its explicit descriptors and descriptor hashes reproduce the bitstreams. Cases
cover 1/2/4/8 token partitions, sparse coefficient categories and signed extremes,
Y2/skip contexts, custom probability updates, absolute/delta segmentation,
quantizer adjustments/clamping, normal/simple filter levels, sharpness and
reference/mode deltas. FFmpeg independently supplies exact YUV expectations.

Lossy RGB reference conversion passes through RGBA16 with explicit nondithered
reduction to RGBA8. The registered 8.1 scalar direct-RGBA8 path wraps blue to zero
for YUV=(255,255,128); its RGBA16 path saturates correctly. Analytical color cases
verify this reference path, including reconstructed samples outside nominal video
range. Pixel tolerances remain maximum RGB error <=4 and RMSE <=1.5, with exact
YUV and alpha; changing the conversion does not weaken those gates.

JPEG expectations use full-range BT.601 conversion and bilinear chroma expansion.
Extract chroma planes, expand by the actual sampling factors, crop to the original
dimensions and merge before color conversion. This preserves the chroma grid for
odd dimensions. See `JpegOracle.cs`, [FFmpeg filters](https://ffmpeg.org/ffmpeg-filters.html)
and [scaler options](https://ffmpeg.org/ffmpeg-scaler.html). Comparisons require RGB
maximum error <=4, RGB RMSE <=1.5, exact dimensions and opaque alpha.

Regenerate with the pinned oracle executable:

```powershell
$env:LOKAD_IMAGE_FFMPEG = './external/tools/ffmpeg.exe'
dotnet run --project tools/Fixtures -c Release -- tests/Fixtures
```

On Linux x86_64, use `bash tools/setup-linux.sh`, source
`external/linux-x64/environment.sh`, and generate into an ignored directory such
as `artifacts/fixture-replay`. The pinned source build records its own tool and
platform identity. Different native zlib builds can produce different PNG
compressed bytes; the decoded samples must still match exactly. The frozen
manifests retain the identity of the executable that produced them.

The small frozen corpus is used offline. Additional packed-depth, palette,
transparency and orientation cases are independently authored binary builders in
the internal tests. Their expected pixels are computed from simple sample grids.
