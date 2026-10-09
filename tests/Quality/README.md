# Original JPEG and VP8 quality and size evidence

## JPEG

`jpeg-quality.json` records 320 cases: five original 129x67 patterns, grayscale
and 4:4:4/4:2:2/4:2:0 profiles, RGBA8/RGBA16 sources, and four settings for each
encoder. Lokad.Image uses quality 1/50/85/100; FFmpeg MJPEG uses qscale 31/16/5/2.
These are independent controls, with no mapping to equal quality or speed.
The report includes source/reference/encoded/decoded hashes, actual component
sampling factors, quantization hashes, encoded bytes, exact native argument vectors
and the registered executable identity. It contains no timing measurements.

The grayscale profiles start with gray samples. Transparent samples are flattened
over 0x202020, combining alpha flattening and depth reduction in one rounding to
opaque RGBA8. Lokad.Image encodes the original 8/16-bit samples and explicit
background; FFmpeg encodes that independently calculated opaque reference.
Its MJPEG encoder accepts three-component YUV rather than one-component grayscale,
so those native rows contain neutral-chroma 4:4:4 and record the actual components.
They do not establish a matched grayscale coding profile.

Native encoding uses full-range BT.601 with area chroma sampling, accurate rounding,
scalar CPU settings, one codec/filter thread and optimal Huffman coding. Chroma
positions are zero on full-resolution axes and 128 on subsampled axes. Analytical
primary-color controls independently verify the 4:4:4 decoded colors and the raw
4:2:2/4:2:0 sampling-cell placement before coding. Native decoding applies the
explicit full-range BT.601 and centered bilinear chroma expansion described in
`tools/Shared/JpegOracle.cs`, preserving the grid at odd dimensions.

Source RGB RMSE/PSNR measure error against the flattened reference. They are
separate from agreement between Lokad.Image and FFmpeg decoding the same bitstream,
which remains bounded by maximum channel error 4 and RGB RMSE 1.5. Alpha is exactly
255. Public API tests reproduce all 160 owned bitstreams, check literal quantization
endpoints at each quality, and establish that encoding the original transparent
8/16-bit source equals encoding its independently flattened RGBA8 reference.

Selected RGBA8 results:

| Original pattern | Sampling | Encoder | Setting | Bytes | RGB RMSE |
| --- | --- | --- | --- | ---: | ---: |
| Gray ramp | Gray | Lokad.Image | quality 85 | 1159 | 0.000 |
| Gray ramp | Neutral 4:4:4 | FFmpeg MJPEG | qscale 5 | 501 | 0.721 |
| Soft color gradient | 4:4:4 | Lokad.Image | quality 1 | 1139 | 8.088 |
| Soft color gradient | 4:4:4 | Lokad.Image | quality 85 | 3621 | 0.725 |
| Soft color gradient | 4:4:4 | Lokad.Image | quality 100 | 7760 | 0.616 |
| Soft color gradient | 4:4:4 | FFmpeg MJPEG | qscale 31 | 706 | 6.133 |
| Soft color gradient | 4:4:4 | FFmpeg MJPEG | qscale 5 | 1078 | 1.825 |
| Soft color gradient | 4:4:4 | FFmpeg MJPEG | qscale 2 | 1484 | 1.251 |
| Color tiles | 4:4:4 | Lokad.Image | quality 85 | 1382 | 0.663 |
| Color tiles | 4:2:0 | Lokad.Image | quality 85 | 972 | 19.316 |
| Asymmetric high-frequency pixels | 4:4:4 | Lokad.Image | quality 100 | 43771 | 0.700 |
| Asymmetric high-frequency pixels | 4:2:0 | Lokad.Image | quality 100 | 22489 | 68.465 |

The first owned encoder uses fixed canonical Huffman tables and an original
frequency-weighted quantization policy. Its output is larger on many of these
inputs. Quality 100 remains lossy, and increasing quantization precision cannot
restore color detail removed by subsampling. Neither source error nor encoded size
is guaranteed to vary monotonically for every pattern or setting. This synthetic
corpus establishes current behavior; it does not establish photographic quality,
competitive compression or performance. The separate
[benchmark calibration](../../benchmarks/README.md) enumerates all owned quality
settings against fixed native anchors, retaining matched pairs and explicit
unmatched targets. Dedicated-VM measurements, broader matched-quality coverage
and compression tuning remain future work.

Regenerate using the registered oracle:

```powershell
dotnet build tools/Fixtures/Fixtures.csproj -c Release --tl:off
dotnet tools/Fixtures/bin/Release/net10.0/Fixtures.dll tests/Quality --jpeg-quality
```

Detailed source and reference recipes are in `tools/Shared/JpegQualityCorpus.cs`;
generation and metadata recording are in `tools/Fixtures/JpegQuality.cs`.

## VP8

`vp8-quality.json` records 96 cases: four independently generated opaque RGBA8
patterns at 129x67, four qualities and three efforts, encoded by Lokad.Image and
the registered FFmpeg/libwebp oracle. Source/encoded hashes, exact argument vectors,
pixel metrics and the oracle executable identity are included. No timing is measured.

Both subjects are independently decoded by FFmpeg with the same explicit chroma
expansion and limited-range BT.601 conversion through RGBA16 followed by explicit
nondithered RGBA8 reduction. This avoids the registered scalar direct-RGBA8 path's
blue overflow at extreme reconstructed samples. RMSE and PSNR include all RGB pixels
against the original source. Alpha must remain 255. Numeric quality and effort
values are encoder-specific; rows at the same number are not matched-quality or
matched-speed comparisons.

Selected effort-4 results (bytes / RGB RMSE):

| Original pattern | Quality | Lokad.Image | FFmpeg/libwebp |
| --- | ---: | ---: | ---: |
| Gray ramp | 1 | 228 / 3.962 | 110 / 3.378 |
| Gray ramp | 85 | 234 / 1.042 | 202 / 0.839 |
| Gray ramp | 100 | 672 / 0.539 | 186 / 0.474 |
| Soft color gradient | 1 | 352 / 9.352 | 186 / 4.867 |
| Soft color gradient | 85 | 566 / 2.538 | 414 / 1.897 |
| Soft color gradient | 100 | 2148 / 1.015 | 964 / 1.034 |
| Color tiles | 85 | 1268 / 19.420 | 508 / 19.331 |
| Asymmetric high-frequency pixels | 85 | 10052 / 68.665 | 6810 / 68.514 |
| Asymmetric high-frequency pixels | 100 | 23502 / 68.473 | 10422 / 68.408 |

These results establish the implemented quality behavior and expose compression
limitations. Increasing quality lowers error on ramps/gradients; 4:2:0 sampling
dominates sharp color edges and the high-frequency pattern. The first owned
encoder's fixed models and lack of rate/distortion search leave larger output,
especially at quality 100. Neither size nor error is guaranteed to vary
monotonically for every image or effort. This small synthetic corpus does not
establish photographic quality, a competitive compression ratio, or performance.

Regenerate using the pinned Windows oracle:

```powershell
dotnet build tools/Fixtures/Fixtures.csproj -c Release --tl:off
dotnet tools/Fixtures/bin/Release/net10.0/Fixtures.dll tests/Quality --quality
```

The detailed source recipes are in `tools/Fixtures/Vp8Quality.cs`. Statistical
timing and tuning await the dedicated quiet VM.
