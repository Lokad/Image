# Warm FFmpeg reference

Lossy VP8 warm preparation uses `fixed-anchor-v1`: native quality 75, managed
quality 100, effort 4, the existing original RGBA8 raster and the unchanged
0.25 dB source-PSNR band. Prepare exactly one owned setting; no quality sweep.
The gate recomputes the match and enforces exact alpha and registered decoder
agreement. Report both packet sizes alongside timing; equal source quality
does not establish equal compression efficiency.

After a source overlay or rollback that preserves file timestamps, force a
Release rebuild before preparation: `dotnet build benchmarks/Lokad.Image.Benchmarks
-c Release -t:Rebuild --no-restore --tl:off`. Incremental build can retain a
superseded DLL. Verify every production method body and implementation flag
against the accepted actual package before profiling or measurement, and check
each proof before reporting success. Freeze the four actual child files and
original receipts before another preparation or source mutation.

RGBA16 bilinear comparisons may explicitly use `--allow-alpha-rounding1`:
native alpha maximum error 1 and RMSE <=0.25 in full 16-bit sample units.
The worker still refuses the original raster in its default exact-alpha mode,
rejects this option at depth 8, and validates RGB maximum 1 / RMSE <=0.25,
zero-alpha RGB, source integrity and independent output ownership before ready.
The managed Down16 benchmark must match the exact rational reference; the
tolerance applies only to FFmpeg's float-stage rounding. Down8 retains exact
native alpha. Both normal and scalar CLI outputs must match the native worker.
Run `verify-bilinear16-linux.py` and `verify-bilinear-linux.py` before measuring.

PNG RGB8/16 encoding has an explicit `png-rgb-encode` operation. It accepts the
existing opaque RGBA8/16 projection and includes single-thread RGBA-to-RGB24 or
RGBA64LE-to-RGB48BE packing inside every native encode operation. The converted
frame/context/wrappers are
reused; independent packet acquisition/release remains included. Full pixels,
source integrity and held owners are checked outside timing. The
[boundary evidence](../../benchmarks/WARM-PNG-RGB8-ENCODE-BOUNDARY.md) remains
unscored because the existing 5% IDAT-size gate fails despite identical filtered
scanlines and level 6/None. Do not interpret preparation or correctness controls
as performance measurements.
The [RGB16 result](../../benchmarks/WARM-PNG-RGB16-ENCODE.md) independently
confirms its fixed pair, including native packing and matched payload sizes.

`png-gray-encode` accepts the frozen opaque grayscale RGBA8 projection and times
single-thread full-range RGBA-to-GRAY8 conversion inside every encode operation.
Equal RGB and opaque input are validated before ready; exact samples and held
owners remain required. Native packet acquisition/release is included. The
[gray8 boundary evidence](../../benchmarks/WARM-PNG-GRAY8-ENCODE-BOUNDARY.md)
remains unscored because the existing IDAT-size gate fails despite exact scanlines
and level 6/None. Allocations are unmeasured.

Maintain [docs/benchmark.md](../../docs/benchmark.md) after every accepted round.
Its [renderer](../../benchmarks/render-benchmark.py) selects retained measurements
and allocations; run it with `--check` before committing. Keep failed quiet guards,
unmatched outputs and unmeasured cases visibly open, and retain their raw evidence.

## Prepared short screens and confirmation

Compile, generate the BenchmarkDotNet child and validate it before the quiet
interval. For a new case, prepare the fixed short or confirmation job on the VM:

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily VerticalFlip -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily VerticalFlip -WarmCases Whole16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

Preparation generates and executes the actual three-iteration WarmShort or
twelve-iteration WarmConfirm job
with one native operation before and after for execution/output validation.
Its receipt is explicitly validation-only, with a null parity ratio. Retain
the generated files; allow preparation activity to settle before a fresh guard.
This supplies a compatible child without compiling inside the scored window.
A retained compatible `WarmShort` or `WarmConfirm` child can run directly without
rebuilding the host. Verify its generated entry selects the declared method,
case and job, and smoke-check its current assemblies first. Keep the generated
files ignored; freeze the actual child, benchmark/library DLLs, runtime config,
owned source inventory and native identity. Never reuse a child for a different
method or case without verifying its generated dispatch.
The existing `-PrepareWarmConfirm` switch remains an alias for confirmation
preparation and still requires `-WarmConfirm`.

On the exclusively used dedicated VM, a controller sets these values and runs
`python3 tools/WarmReference/capture-vm-environment.py .` immediately before and
after the process. Protocol `exclusive-vm-idle-v2` requires no overlapping live
build, codec or profiler job and a five-second idle sample on both CPU0 and the
aggregate CPUs: busy <=5%, I/O wait <=1%, steal <=1%. Load averages are recorded
as diagnostics only. Previous work on this VM does not require waiting for the
historical load average to decay. Compile and prepare outside these intervals.
Earlier receipts used an additional one-minute-load <=0.25 guard; their failures
remain failures under their recorded protocol and are not reclassified.

```bash
export LOKAD_IMAGE_BENCHMARK_RUN_ID=unique-confirmation-id
export LOKAD_IMAGE_WARM_MODE=confirm LOKAD_IMAGE_WARM_CASES=Down16 LOKAD_IMAGE_WARM_CPU=0
export LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS=50
export DOTNET_PROCESSOR_COUNT=1 DOTNET_TieredCompilation=0
export DOTNET_GCServer=0 DOTNET_gcConcurrent=0
# prepared_child points to the verified retained WarmConfirm generated DLL.
taskset -c 0 dotnet "$prepared_child" \
  --benchmarkName 'WarmNearestResize.Managed(Case: Down16)' --job WarmConfirm --benchmarkId 0
```

The optional native calibration target accepts only `50` or `200` milliseconds;
the historical default is `200`. Both count whole operations, keep three native
warmups, and keep twelve pre/post samples for confirmation. The shorter declared
target reduces sustained work without excluding allocation or processing. Record
the setting, raw elapsed/count values, both guards and actual binary identities.
A failed guard stays failed. A preparatory failure launches no benchmark;
subsequent attempts require a fresh guard and unique evidence identity.

The standalone child emits console measurements rather than the host's full
JSON export. Recover them with the pinned BenchmarkDotNet APIs:

```powershell
dotnet build tools/WarmReference/ParseBenchmarkLog/ParseBenchmarkLog.csproj -c Release --tl:off
dotnet tools/WarmReference/ParseBenchmarkLog/bin/Release/net10.0/ParseBenchmarkLog.dll confirm.log confirm-statistics.json
```

The parser defaults to one twelve-iteration confirmation; explicitly add `3`
for the fixed short job. It retains actual and adjusted result rows and refuses
to overwrite evidence. Both modes reproduce retained official report statistics.
This recipe supplied the independent
[RGBA16 nearest confirmation](../../benchmarks/WARM-PREPARED-CONFIRM.md);
it does not certify other cases or earlier failed intervals.

For a short screen, use `LOKAD_IMAGE_WARM_MODE=measure`, the verified `WarmShort`
child and `--job WarmShort`; both native brackets contain three samples. Preserve
the declared method and case consistently across preparation and replay. Every
statistical attempt needs a unique receipt and fresh guards.

## Worker contract

`ffmpeg-warm.c` is an original development-only adapter using public FFmpeg
filter APIs. No implementation code or tables are copied from upstream. It is
not a library build input, native binding or NuGet asset. Build the pinned Linux
oracle first, then compile this worker against its ignored installed libraries:

```bash
bash tools/WarmReference/build-linux.sh
python3 tools/WarmReference/verify-linux.py --cpu 0
```

The executable and build identity stay under ignored `external/warm-reference/`.
The initial operation is horizontal flip at RGBA8/RGBA16LE. The worker loads
one original raster before timing and keeps a single-threaded hflip graph and
source/frame wrappers. Each operation produces fresh non-aliasing result pixels
and releases them. Native allocator pools may reuse released storage; no source
pixels or edited results are cached to replace processing.

Both subjects expose owned interleaved RGBA rows. FFmpeg's allocation alignment
can add row padding; input/output strides are recorded. The library has tight
rows. No extra copy is added solely to remove native padding. File transport and
canonical byte packing happen only in untimed validation, and every channel,
including zero-alpha hidden RGB, must match independent pixel placement.

The bounded line protocol reports ready/thread/affinity evidence, internally
timed `sample COUNT` operations, untimed `check` output/source verification and
`quit`. A worker refuses to run without affinity to exactly one logical CPU.
Native monotonic samples include submitting the source, acquiring/materializing
the owned result and releasing it. Process launch, IPC, hashing and file I/O are
outside the scored interval. The controller must use the reported elapsed
time/count; timing a protocol request with BenchmarkDotNet would measure a
different boundary.

Run `./tools/WarmReference/capture-source.ps1` from PowerShell before execution;
it inventories the owned tracked/unignored files under ignored `tmp/`.
`./bench.ps1 -Smoke -Warm -Cpu 0 -NoBuild` validates execution/output on Linux.
`./bench.ps1 -DedicatedVm -Warm -Cpu 0 -NoBuild` measures the two precision cases
on the quiet VM. Managed
workers use processor count one, workstation GC and no background GC, with
a fixed fully optimized JIT policy and tiered compilation disabled, so
optimization cannot be deferred into measurements on the single busy CPU. Both
subjects are pinned to the chosen allowed logical CPU. Native pre/post samples
bracket the managed BenchmarkDotNet samples. Statistics, identities, exact checks
and raw native elapsed/count values remain under ignored benchmark artifacts.
Screened ratios need independent repeats before parity is certified.
If short-run uncertainty overlaps the target, add `-WarmConfirm` to the dedicated
VM command. It keeps the same two inputs and boundary, extending both managed
and native measurements from three to twelve samples per case.
The verification command reuses exactly the two declared inputs, validates
sample counters and every pixel, and requires rejection of multi-CPU affinity,
zero/excessive counts, trailing protocol tokens and unknown commands.

## PNG

`ffmpeg-png-warm.c` is a separate original adapter for public FFmpeg codec APIs:

```bash
bash tools/WarmReference/build-png-linux.sh
python3 tools/WarmReference/verify-png-linux.py --cpu 0
# After a Release benchmark build, author one existing-raster Adam7 input.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-png-adam7-16
python3 tools/WarmReference/verify-png-adam7-linux.py --cpu 0
# Or author one existing-raster noninterlaced Paeth input.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-png-paeth-16
python3 tools/WarmReference/verify-png-paeth-linux.py --cpu 0
# Or prepare the single eight-bit Paeth input on the existing eight-bit raster.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-png-paeth-8
python3 tools/WarmReference/verify-png-paeth-linux.py --cpu 0 --depth 8
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -Cpu 0 -NoBuild
# Iterate one measured gap without adding images or unrelated cases.
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Encode16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeAdam7_16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodePaeth16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodePaeth8 -Cpu 0 -NoBuild
```

The default pair is RGBA8 decode. Encode8, Encode16, Decode16, DecodeAdam7_16, DecodePaeth16 and DecodePaeth8
are explicitly selectable on the same two original 513x259 rasters as horizontal
flip; any selection is capped at three distinct pairs. Both decoders consume identical
resident PNG bytes and produce independent owned RGBA pixels. Native 16-bit
decode includes acquiring a second owned frame and converting big-endian PNG
samples to little-endian RGBA16; padded rows need no packing copy. Managed decode
includes the complete public call, allocation, validation and disposal.

Encode uses compression level 6, filter None and noninterlaced RGBA on both
subjects. Full inflated rows must match, and prepared IDAT payload sizes must
be within 5% before timing. Managed output is a fresh MemoryStream, including
creation, growth, encoding and disposal; no redundant ToArray is timed. Native
encoding includes independent packet acquisition, encoding and release.
Contexts, resident input and native wrappers are reused outside the intervals.
RGBA16 encoding includes little-to-big-endian conversion of every sample inside
each native operation, using a reused prepared big-endian frame. Managed row
conversion/storage is included in the public EncodePng call. The
[bulk encoder CRC round](../../benchmarks/WARM-PNG16-ENCODE-CRC.md) changes only
managed chunk CRC computation and preserves the entire encoded PNG byte stream.
For independent still PNGs, every native encoded frame is forced intra and
every decoded input is marked key. Reuse without those settings can exercise
sequence delta frames and fails the intended workload.

Before timing, two simultaneously live results are checked for independent
storage and exact samples. Repeated complete operations, output sizes, counters,
all PNG row filters, alpha and hidden RGB are verified outside timing. The
verification script also tests twenty protocol/affinity rejection controls.
Native PNG binaries and their separate build identities remain ignored under
external/warm-reference/. The single-CPU, GC/JIT and quiet-VM policy is identical
to horizontal flip; three-sample screens are priorities, not certification.
The [Adam7 RGBA16 baseline](../../benchmarks/WARM-PNG-ADAM7-BASELINE.md) retains
one authored seven-pass/filter-None input and exact normal/scalar CLI samples.
Its independent quiet confirmation remains above2x. A six-second profile selects
generic sparse-pixel conversion and repeated destination access for the next
substantial primitive. Earlier failed quiet guards remain failed.
The [sparse RGBA reconstruction round](../../benchmarks/WARM-PNG-SCATTER.md)
then reduces complete decode20%, independently clearing this fixed pair at1.648x
FFmpeg, conservative upper1.700x. Both timing libraries match all563 production
methods and flags in the same actual package verified on both OSes. Exact samples,
allocations, admission and cancellation are preserved; no workspace is added.
The [Paeth RGBA16 baseline](../../benchmarks/WARM-PNG-PAETH-BASELINE.md) uses one
fixed all-Paeth/noninterlaced input on the existing raster. Its quiet screen
exposes2.604x FFmpeg, upper2.721x. A six-second profile attributes about46% of
sampled CPU to Paeth prediction/unfiltering, selecting grouped channel reconstruction
for one substantial primitive. Exact analytical and normal/scalar CLI samples pass.
The [eight-channel Paeth round](../../benchmarks/WARM-PAETH-VECTOR.md) then reduces
complete decode39.2%, independently clearing this fixed pair at1.548x FFmpeg,
upper1.607x. All564 production methods/flags match the actual package validated
on both OSes. Thirteen normal/scalar controls include every predictor triple,
aliases, tails, errors and cancellation. One guard-failed confirmation is frozen;
a fresh process with stricter starting load passes the same unchanged guard limits.
The [Paeth RGBA8 baseline](../../benchmarks/WARM-PNG-PAETH8-BASELINE.md) uses the
existing eight-bit raster with the same fixed filter/effort. Its quiet three-sample
mean is2.071x, upper7.146x; no parity confirmation is claimed. A short profile
selects four-channel reconstruction. Generalized preparation retains the exact
previous RGBA16 input bytes; default selection remains one Decode8 case.

## JPEG

The original `ffmpeg-jpeg-warm.c` adapter uses the public codec and swscale APIs:

```bash
bash tools/WarmReference/build-jpeg-linux.sh
python3 tools/WarmReference/verify-jpeg-linux.py --cpu 0
```

Build Release managed binaries and capture the owned source inventory first.
Prepare exactly one quality anchor outside timing, then execute the narrow pair:

```powershell
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg 0 5
./bench.ps1 -Smoke -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
```

Encode8 reuses the existing 513x259 RGBA8 raster. Both subjects flatten its alpha
onto RGB 0x204060 with the specified single rounding, convert to full-range
BT.601 and encode a complete independent 4:4:4 JPEG. Every timed operation
includes flattening, color conversion, encoding and output acquisition/release.
Native contexts and scratch frames are reused; source processing and encoding
execute again each time. Managed MemoryStream creation, growth and disposal
are included. Native optimal Huffman and managed fixed Huffman are explicit.

Preparation checks one native qscale against at most eight managed settings,
using the existing fixed 0.25 dB source RGB-error band and decoder/alpha bounds.
It retains all settings and unmatched anchors under ignored artifacts. Timing
refuses an unmatched pair. Setup and cleanup freshly validate the selected
bitstreams, geometry, sampling, decoder agreement, quality, independent live
storage and unchanged input; they never enumerate qualities. Changed output,
native worker or source/reference identities require preparing only this pair
again. Prepared codec output is a validation target, never a timed image cache.

The protocol controls reuse one source raster and reject fourteen invalid
requests, arguments, sizes and affinities. They verify deterministic owned
bitstreams, opaque decoded alpha and exact operation counters without publishing
timing claims. Single-CPU quiet-VM and GC/JIT policies match the other warm families.

## JPEG decode

`ffmpeg-jpeg-decode-warm.c` uses the public software `mjpeg` decoder and
single-threaded swscale. It reuses the already prepared managed 513x259,
8-bit 4:4:4 JPEG; both subjects receive identical resident encoded bytes.
The default `Decode8` selects only that input. Explicit `-WarmCases NativeDecode8`
selects only the already prepared native JPEG from the same original raster,
with its variable Huffman tables and common 1x2 component factors (still 4:4:4).
No source raster or encoder-quality enumeration is added.
Explicit NativeDecode420 or NativeDecode422 selects one fixed baseline 4:2:0
or 4:2:2 JPEG from the same
original raster, prepared at native qscale 5 with alpha flattened onto RGB
0x204060. This is identical-input decoding, with no encode quality search.
OwnedDecodeGray selects one owned baseline grayscale JPEG at fixed quality 85
on that same raster. Native MJPEG cannot encode the equivalent one-component
input; FFmpeg supplies the decode oracle for the identical resident bytes.
ProgressiveGray losslessly rewrites that same grayscale input into six scans.
A pinned ignored jpegtran producer and an original coefficient visitor prove
unchanged quantization, coefficient geometry, all DCT values and owned pixels.
Only the encoded scan organization differs; no scan or quality search is added.

```bash
bash tools/WarmReference/build-jpeg-decode-linux.sh
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0
# After a Release benchmark build, prepare this single input once.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg420-decode 0
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0 --sampling 420
# Alternatively, prepare and validate the single 4:2:2 input.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg422-decode 0
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0 --sampling 422
# Or prepare the one-component grayscale input.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg-gray-decode 0
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0 --sampling gray
# One lossless progressive rewrite of the already prepared grayscale input.
bash tools/WarmReference/build-jpegtran-linux.sh
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg-progressive-gray 0
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0 --sampling progressive-gray
# Or rewrite the existing native 4:2:0 input once, without changing quality.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg-progressive-420 0
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0 --sampling progressive-420
# Or rewrite the existing full-resolution native input, retaining its factors.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg-progressive-444 0
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0 --sampling progressive-444
# Or rewrite the existing horizontal-only native input, retaining its factors.
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg-progressive-422 0
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0 --sampling progressive-422
```

After a Release benchmark build and owned source inventory:

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily JpegDecode -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases NativeDecode8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases NativeDecode420 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases NativeDecode422 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases OwnedDecodeGray -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases ProgressiveGray -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases Progressive420 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases Progressive444 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases Progressive422 -Cpu 0 -NoBuild
```

Each native interval resets decoder state, decodes all coefficients/pixels,
converts full-range BT.601 to RGBA8, allocates independent output, and releases
it. Codec/conversion/input packet/frame wrappers are reused. Padded owned
RGBA rows need no tight-row copy inside timing. Managed public Decode,
allocation and disposal are included. Startup, I/O, IPC and checks are excluded.
The 4:2:0 path includes acquiring and expanding both ceil-sized chroma planes by
two, using a crop view at the original bounds, and releasing the intermediates.
This preserves the specified chroma grid at odd dimensions. Contexts and wrappers
are reused; there is no additional full-frame packing copy. The
[eight-pixel render round](../../benchmarks/WARM-JPEG420-BULK.md) records this pair
and one managed interpolation/color primitive with exact owned output bytes.
The 4:2:2 mode expands only horizontally and retains the original row grid,
including intermediate acquisition/release inside each operation. Its
[reused-row round](../../benchmarks/WARM-JPEG422-ROWS.md) supplies the same chroma
row twice to the existing eight-pixel renderer, preserving exact interpolation
and color bytes without adding a workspace or method.
Grayscale decoding includes expanding owned GRAY8 samples into independent
RGBA8 inside each operation. The software decoder leaves GRAY8 color range
untagged; the conversion explicitly uses full JPEG range. Only this grayscale
representation admits an unspecified range tag. The
[direct byte-expansion round](../../benchmarks/WARM-JPEGGRAY-PIXELS.md) preserves
the former rounded/clipped samples, work, admission and output ownership.
The [progressive accounting trial](../../benchmarks/WARM-JPEGPROGRESSIVE-ACCOUNTING-REJECTION.md)
records this six-scan pair and rejects a small uncertain local-accounting change.
The [EOB-tail correction trial](../../benchmarks/WARM-JPEG-CORRECTION-REJECTION.md)
is flat despite passing quiet guards. Its separate CPU diagnosis targets the
dominant progressive scan loop; diagnostic sample shares do not certify parity.
The [grouped refinement round](../../benchmarks/WARM-JPEG-REFINEMENT.md) retains
spectral layout and safe grouped corrections after independent quiet confirmation
clears this fixed pair. Both actual timing libraries match the final actual
package's complete production methods and flags; work/pixels/admission stay exact.
The fixture producer is used only outside timing,
and its checkout/build/binaries stay ignored under external/.
The [progressive 4:2:0 coverage point](../../benchmarks/WARM-JPEG-PROGRESSIVE420.md)
proves all valid three-component coefficients/quantization/geometry and owned
pixels unchanged. One independent confirmation clears 2x with no codec change;
the conservative bound also includes all twelve actual measurements before
BenchmarkDotNet's result selection or overhead subtraction.
The [progressive full-resolution baseline](../../benchmarks/WARM-JPEG-REFINE16-REJECTION.md)
retains the existing common sampling factors and all valid coefficients/pixels.
It exposes a 2.13x gap. A wider correction-group trial is slower and is restored;
the [per-block mask round](../../benchmarks/WARM-JPEG-MASK.md) then reduces
confirmed managed time 12.4%, clearing this fixed pair with an all-actual upper
1.878x and passing independent quiet guards. Earlier failed guards remain failed.
No additional raster or quality/scan-strategy search is added.
The [progressive 4:2:2 coverage point](../../benchmarks/WARM-JPEG-PROGRESSIVE422.md)
retains all 276,672 valid signed coefficients, quantization, shared vertical
sampling factors and owned pixels. Its independent quiet confirmation is 1.757x
FFmpeg, conservative upper 1.781x including all twelve actual measurements.
Production methods and flags remain identical to the accepted actual package.
Two simultaneously live outputs must have independent storage; mutation of
one leaves the other intact. Repeated output pixels and encoded source bytes
must remain unchanged. Fifteen invalid protocol/argument/affinity controls per
sampling mode include rejecting wrong declared sampling. Setup verifies the native pixels exactly against the pinned
public FFmpeg CLI with the same conversion flags and normal SIMD; managed/native
agreement retains RGB maximum 4/RMSE 1.5 and exact opaque alpha. Setup also verifies
managed agreement against the pinned scalar CLI under those bounds. Decoder timing
does not require an encoder-quality match because its encoded input is identical.

## Still VP8 WebP

The original `ffmpeg-webp-warm.c` worker calls the public still `libwebp`
encoder through FFmpeg codec APIs. Its packet contains the complete RIFF WebP:

```bash
bash tools/WarmReference/build-webp-linux.sh
python3 tools/WarmReference/verify-webp-linux.py --cpu 0
```

After the Release build and owned source inventory, prepare one anchor and run
one pair:

```powershell
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-webp 0 75
./bench.ps1 -Smoke -Warm -WarmFamily Webp -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Webp -Cpu 0 -NoBuild
```

The first quality-75 preparation is [unmatched](../../benchmarks/WARM-WEBP-MATCH.md).
Smoke/timing therefore cannot accept it. Run the benchmark executable with
`--validate-warm-webp-controls` to check the preparation gate without encodes or
native tools; eleven synthetic invalid records must be rejected before a worker
can start. Retrieve and retain the full catalog when extending this evidence.

Encode8 reuses the existing 513x259 RGBA8 raster, including transparency. Each
native operation repacks resident RGBA into its reused BGRA scratch frame,
encodes all pixels/alpha, acquires complete independent WebP bytes, and releases
the packet. The scalar repacking loop executes on the same CPU/thread; native
SIMD dispatch remains enabled. Codec/frame/packet wrappers are reused. Managed
MemoryStream creation, growth, encoding and disposal are timed. Neither subject
caches converted pixels or encoded bytes to replace work.

Both effort settings are fixed at 4. Native quality is explicitly set with no
global-quality override and the `none` preset; animation/lossless encoders are
excluded. Native adaptive coding and managed fixed entropy are declared. Alpha
must reproduce exactly. Quality uses the existing integer alpha-weighted RGB
source error before composite rounding; decoder agreement retains unweighted
maximum 4/RMSE 1.5 bounds including hidden RGB. At most eight managed settings
are prepared against one native anchor, with fixed maximum 0.25 dB gap.
Unmatched anchors remain recorded and cannot be timed. Setup and cleanup freshly
check selected bitstreams, geometry, quality, source integrity and ownership.

Protocol controls reuse this one raster, check complete still VP8/alpha RIFF
framing, independent live packet storage, alpha and operation counters, and
reject fourteen invalid requests/arguments/affinities. Native binaries, static
archives and identities remain ignored. Measurement requires the quiet VM and
the same one-CPU, GC/JIT, bracketing and repeat policy as the other warm families.

## Still VP8L WebP encode

`WarmWebpLossless/Encode8` uses **libwebp exact=1 (FFmpeg encoder backend)** as
the explicitly approved exact lossless baseline. FFmpeg's public libwebp wrapper
has no exact-RGB option and changes hidden RGB on the retained raster. Its
original adapter, `ffmpeg-webp-lossless-warm.c`, is retained for this mismatch
diagnosis; it does not supply parity timings. The original
`libwebp-lossless-warm.c` adapter uses only libwebp's public encoding API.
No upstream implementations or tables are copied; production has no bindings.

```bash
bash tools/WarmReference/build-libwebp-lossless-linux.sh
python3 tools/WarmReference/verify-webp-lossless-linux.py --cpu 0
```

After a Release benchmark build and owned source capture, prepare the one case:

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily WebpLossless -WarmCases Encode8 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily WebpLossless -WarmCases Encode8 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

Reuse the existing original 513x259 RGBA8 raster, including hidden RGB. Managed
Lossless=true/Effort=4 and native lossless=1/method=4/quality=75/exact=1 are fixed;
quality is a lossless compression control. Syntax, managed decode and normal/
scalar FFmpeg decode must reproduce every RGBA byte before timing. Compression
ratios need not match and output sizes must accompany the comparison.

Each native call repacks every pixel into reusable BGRA scratch, freshly imports
and allocates picture pixels, encodes to independently owned bytes and releases
pixels/packet. Only config/picture/writer wrappers and BGRA scratch are reused.
Managed timing includes admission and fresh MemoryStream creation, growth and
disposal, without ToArray. Output checks, startup, I/O and IPC are excluded.
One CPU, thread_level=0, normal SIMD and the same quiet bracketing policy apply.
The [retained comparison](../../benchmarks/WARM-VP8L-ENCODE-EXACT.md) declares
the substantial compression difference, allocation and failed pre-run guard.

## Native-input VP8L decode

`WarmWebpDecode/NativeLossless8` freezes one fixed libwebp exact=1/method=4/
quality=75 encoding of the same original RGBA8 raster. It adds native adaptive
coding coverage beyond `Lossless8`; the prior input and default selection remain
unchanged. No additional performance image or encoder-setting sweep is needed.

```bash
python3 tools/WarmReference/prepare-webp-native-decode-linux.py --cpu 0
python3 tools/WarmReference/verify-webp-decode-linux.py --cpu 0 --native-input
```

Preparation refuses to overwrite its ignored `webp-native-decode/` directory
and requires the exact input identity established by the encode baseline. Both
normal and scalar FFmpeg decoding must reproduce every original RGBA sample,
including hidden RGB. Setup independently checks managed pixels, simultaneous
owned results, native output and normal/scalar CLI agreement before timing.

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily WebpDecode -WarmCases NativeLossless8 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily WebpDecode -WarmCases NativeLossless8 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

Decode timing uses FFmpeg's public software decoder, with exactly the same
frozen input on both subjects. The approved encoder-backend exception applies
only to the untimed preparation. Public Decode/admission/entropy/transforms,
ARGB-to-RGBA conversion and independent output acquisition/release remain in
their respective boundaries; native wrappers/contexts are reused. No decoded
pixels are cached. The [canonical lookup round](../../benchmarks/WARM-VP8L-CANONICAL.md)
retains source identities, all actuals, allocation and failed guards.

## Still VP8L WebP decode

The original `ffmpeg-webp-decode-warm.c` adapter uses FFmpeg's public software
`webp` decoder and public ARGB-to-RGBA conversion APIs. It accepts one complete
version-zero VP8L RIFF input. Build the worker and the Release solution, then
capture the owned source inventory:

```bash
bash tools/WarmReference/build-webp-decode-linux.sh
dotnet build Lokad.Image.slnx -c Release --tl:off
```

```powershell
./tools/WarmReference/capture-source.ps1
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-webp-decode 0
```

Preparation runs once, outside every benchmark launch, and refuses to overwrite
its manifest. It freezes one original Lokad.Image VP8L encoding of the existing
513x259 RGBA8 raster at effort 4 under ignored `artifacts/warm-reference/webp-decode/`.
Both Lokad.Image and the pinned FFmpeg CLI must reproduce every original RGBA
byte, including alpha and zero-alpha hidden RGB. The selected profile has no
transforms, color cache or spatial entropy groups. No quality matching or
additional source image is needed for identical-input lossless decoding.

```bash
python3 tools/WarmReference/verify-webp-decode-linux.py --cpu 0
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily WebpDecode -WarmCases Lossless8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily WebpDecode -WarmCases Lossless8 -Cpu 0 -NoBuild
```

Run measurement sequentially on the quiet dedicated VM; check quietness before
and after each run and repeat this one pair. Setup consumes the frozen bytes
without encoding again. Managed timing covers the full public span Decode call,
independent output acquisition and disposal. Native timing includes decoder
state reset, every decoded pixel, ARGB-to-RGBA conversion and independent owned
output allocation/release. Codec/conversion contexts and packet/frame wrappers
are reused; padded output rows require no extra packing copy. Neither subject
caches decoded pixels. Startup, IPC, I/O, generation, hashing and validation are
excluded. CPU affinity, codec and conversion threads are each one; thread types
are zero and normal SIMD remains enabled.

Twenty negative controls check affinity, arguments, protocol, geometry and RIFF
framing using the same frozen input. Successful controls check operation counts,
source integrity, independent live native output storage and exact public-CLI
pixels. Setup and cleanup check complete managed/native RGBA and source integrity.
The [lookup inlining round](../../benchmarks/WARM-VP8L-READ-INLINE.md) reuses this
same input and worker, reaching about 1.74x with unchanged production method
bodies and exact RGBA. Independent quiet confirmation remains open.
The [retained screens](../../benchmarks/WARM-WEBP-DECODE.md) cover this one VP8L
profile only. Other VP8L strategies remain unmeasured. This native
adapter, its ignored binaries and static archives are development-only and never
production build inputs.

## Warm native-input VP8 decode

`WarmVp8Decode/Native8` reuses the quality-75 native VP8/ALPH file already frozen
in ignored `artifacts/warm-reference/webp-match.json`. It requires that exact
input and original raster identity. It neither regenerates the input nor searches
encoder qualities. An unmatched encode pair does not prevent identical-input
decode comparison.

```bash
bash tools/WarmReference/build-vp8-decode-linux.sh
python3 tools/WarmReference/verify-vp8-decode-linux.py --cpu 0
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Vp8Decode -WarmCases Native8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Vp8Decode -WarmCases Native8 -Cpu 0 -NoBuild
```

The original development adapter uses software FFmpeg webp decoding, normal
SIMD, one codec thread and one thread per conversion context. Its timed operation
resets decoder state, decodes every pixel, expands centered chroma bilinearly,
converts limited-range BT.601 through RGBA16, reduces to RGBA8 without dither,
merges exact alpha, and allocates/releases scratch and independent owned output.
Contexts and packet/frame wrappers are reused. Managed timing covers the public
span Decode and output disposal. Startup, I/O, IPC, encoding and validation are
excluded. Native output must match the pinned CLI exactly. Managed RGB, including
hidden RGB, must satisfy the specified maximum 4/RMSE 1.5 bounds; alpha is exact.
Both retain frozen pixel hashes and test distinct live outputs and source integrity.
Seventeen negative controls cover affinity, arguments, protocol, geometry and
framing. Run one pair sequentially on the quiet dedicated VM and archive its
source, raw samples and actual timed library before changing source.
The [retained VP8 screen](../../benchmarks/WARM-VP8-NATIVE-DECODE.md) covers only
this frozen native keyframe/alpha profile; broader VP8 coverage remains open.
The [reader inlining round](../../benchmarks/WARM-VP8-READ-INLINE.md) reuses the
same native worker, input and correctness boundary, with no encode-quality search.
The [branchless interval-selection round](../../benchmarks/WARM-VP8-BRANCHLESS.md)
continues that same pair and preserves its pixel, ownership and thread contract.
The [fixed coefficient-token round](../../benchmarks/WARM-VP8-COEFFICIENT-TOKEN.md)
retains those boundaries and replaces generic tree traversal with format-specific
decisions. No additional image or encoder-quality enumeration occurs.
The [eight-pixel conversion round](../../benchmarks/WARM-VP8-COLOR-BLOCK.md)
retains exact scalar pixels with a safe SIMD interior kernel. Complete warm
decode screens reach about 1.94x; independent quiet confirmation remains open.
The [reader-state caching trial](../../benchmarks/WARM-VP8-READER-STATE-REJECTED.md)
is discarded as flat, without a longer repeat. Its offset controls remain.
The [integer chroma-expansion round](../../benchmarks/WARM-VP8-CHROMA-INTEGER.md)
preserves the exact original samples and matrix on that pair. Scoped work caching
was flat and discarded; its original trial evidence remains available.

## Warm clockwise quarter turn

`WarmClockwise/Whole16` reuses the existing original RGBA16 513x259 raster,
producing 259x513. Build the original public-API adapter and validate full-word
samples, source, independent live owners and fourteen rejection controls:

```bash
bash tools/WarmReference/build-clockwise-linux.sh
python3 tools/WarmReference/verify-clockwise-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Clockwise -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Clockwise -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
```

Native transpose uses clockwise placement with passthrough disabled, allocating
and processing independent output pixels before release inside every operation.
Graph/source/frame wrappers are reused. Padded rows need no packing or extra
materialization copy. Managed timing includes complete Rotate(Clockwise90),
admission, materialization and disposal. Both sides may reuse released storage;
neither caches outputs. Full precision, alpha, hidden RGB, source and live
owners are exact, as are pinned normal/scalar CLI pixels. Use one CPU/filter
thread, thread type zero, normal SIMD and the unchanged quiet-VM policy.

The [bounded-tile round](../../benchmarks/WARM-CLOCKWISE-TILES.md) brings the
screened mean from 26.57x to 1.00x, with conservative short upper 1.51x and both
short guards passing. Its longer confirmation and coverage limits are reported
separately: twelve samples pass both guards at 0.990x, with conservative upper
below 1.014x, certifying this fixed pair. `Whole8` is an explicit correctness selection; only `Whole16` is
measured in this round. No image or geometry sweep is added.

## Warm half turn

`WarmHalfTurn/Whole16` reuses the existing original RGBA16 513x259 raster.
Build the original public-API adapter and validate full-word samples, source,
independent live owners and fourteen rejection controls:

```bash
bash tools/WarmReference/build-halfturn-linux.sh
python3 tools/WarmReference/verify-halfturn-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily HalfTurn -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily HalfTurn -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
```

Native hflip allocates independent pixels; vflip's allocation callback lets it
place rows in reversed order during that pixel pass. The observed output has
positive padded stride. The worker accepts either stride sign, verifies every
pixel and owner, then releases output inside each timed operation. It adds no
redundant full copy to enforce positive stride. Managed timing includes the
complete public Rotate(Half), admission, materialization and disposal. Both
sides may reuse released storage; neither caches output pixels. Graph/source/
frame wrappers are reused. Full precision, alpha, hidden RGB, source and live
owners are exact, as are pinned normal/scalar CLI pixels. Use one CPU/filter
thread, thread type zero, normal SIMD and the unchanged quiet-VM policy.

The [complete-pixel round](../../benchmarks/WARM-HALFTURN-PIXELS.md) brings the
screened mean from 53.87x to 0.90x, with conservative short upper 1.53x and
both short guards passing. Its longer confirmation and coverage limits are
reported separately: the twelve-sample repeat passes both guards at 0.933x,
conservative upper 0.967x, certifying only this fixed pair. `Whole8` is an explicit correctness selection; only
`Whole16` is measured in this round. No image or geometry sweep is added.

## Warm vertical flip

`WarmVerticalFlip/Whole16` reuses the existing original RGBA16 513x259 raster.
Build the original public-API adapter and validate exact full-word samples,
independent owners and fourteen rejection controls:

```bash
bash tools/WarmReference/build-vflip-linux.sh
python3 tools/WarmReference/verify-vflip-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily VerticalFlip -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily VerticalFlip -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
```

Native vflip produces a shared negative-stride view. Each timed operation
allocates an independent frame, copies every reversed row and releases the
view/output. Managed timing includes the complete public call, admission,
materialization and disposal. Both sides may reuse released storage; neither
caches output pixels. Graph/source/frame wrappers are reused. Full samples,
alpha, hidden RGB, source and simultaneous owners are exact, as are pinned
normal/scalar CLI pixels. Use the same single CPU/filter thread, normal SIMD
and quiet-VM policy as other families. `Whole8` is an explicit correctness
selection; only `Whole16` is measured in this round.

The [reversed-row round](../../benchmarks/WARM-VFLIP-ROWS.md) brings the screened
mean from 67.28x to 0.99x. Two twelve-sample repeats clear the numerical 2x bound
but fail their post-run quiet guards; independent quiet confirmation remains
open. The accepted source and actual binaries are frozen with every raw attempt.
It adds no performance raster or geometry sweep. Other transforms remain open.

## Warm owned crop

`WarmCrop/Fixed16` reuses the existing original RGBA16 513x259 raster and one
rectangle at x=127,y=65, width=256,height=129. Build the original public-API adapter
and validate its exact samples, independent owners and fourteen rejection controls:

```bash
bash tools/WarmReference/build-crop-linux.sh
python3 tools/WarmReference/verify-crop-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Crop -WarmCases Fixed16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Crop -WarmCases Fixed16 -Cpu 0 -NoBuild
```

Native crop produces a view, so every timed operation allocates a new owned frame,
copies all cropped pixels and releases the view/output. The managed complete
public call likewise includes admission, materialization and disposal. Graph,
source and native frame wrappers are reused; source processing and output copying
repeat every time. Released storage may be reused on either side. Source, full
UInt16 values, alpha, hidden RGB, simultaneous owners and pinned CLI pixels are
exact. Startup, IPC, I/O, generation and checks are excluded; use the same single
CPU/filter-thread, normal-SIMD and quiet-VM policy as other warm families.

The [row-copy and reusable-storage round](../../benchmarks/WARM-CROP-COPY.md)
screens two substantial primitives separately, bringing this pair from about
70x to 0.99x with conservative upper 1.0031x and independent quiet confirmation.
It adds no performance raster or rectangle sweep. Both sample depths have
focused correctness controls; only this RGBA16 rectangle is timed. Other
rectangles/transforms retain their coverage limits.

## Warm nearest resize

`WarmNearestResize/Down8` and the explicit `Down16` selection each cover one
513x259 RGBA8 or RGBA16 to 256x129 nearest resize at the same sample depth
on the existing original raster. It preserves exact integer center coordinates,
alpha and zero-alpha hidden RGB. No extra source image or quality search occurs.

```bash
bash tools/WarmReference/build-nearest-linux.sh
python3 tools/WarmReference/verify-nearest-linux.py --cpu 0
# Reuse the existing RGBA16 precision-audit input for full-word controls.
python3 tools/WarmReference/verify-nearest-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Nearest -WarmCases Down8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Nearest -WarmCases Down8 -Cpu 0 -NoBuild
# Select one depth per run; Down8 remains the default.
./bench.ps1 -Smoke -Warm -WarmFamily Nearest -WarmCases Down16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Nearest -WarmCases Down16 -Cpu 0 -NoBuild
```

The original development adapter uses public FFmpeg filter APIs with one filter
thread, thread type zero and normal SIMD. The timed operation includes packed
and planar conversion, plane extraction, separate RGB and replicated-alpha zscale
point kernels, merging and independently owned output pixel acquisition/release.
The graph, geometry contexts, immutable source and frame wrappers are reused;
every operation processes every selected sample. Padded output rows need no extra
packing copy. Managed timing covers the complete public Resize call, admission,
allocation, materialization and disposal. Startup, IPC, I/O, hashing, generation
and validation are excluded. Neither subject caches output pixels.

Fourteen negative controls cover affinity, arguments, geometry, precision, input
framing and bounded protocol. Operation counters, exact pinned CLI pixels,
source integrity and two simultaneously live independent outputs also pass.
The [first screen](../../benchmarks/WARM-NEAREST-BASELINE.md) reaches about 1.31x
without a production change. The packed swscale-neighbor CLI alternative fails
exact selected pixels and is not timed. Run sequentially on the dedicated quiet
VM, preserving source, raw samples and the actual statistical DLL before edits.
The RGBA16 path uses RGBA64LE/GBRP16LE/GBRAP16LE throughout and validates every
full-word sample against the integer reference. Its
[specialized row kernel](../../benchmarks/WARM-NEAREST-RECURRENCE.md) removes
per-pixel division/callbacks and reduces the screened mean 70.78%, reaching
0.62x with conservative upper 0.64x on Down16. No extra performance raster is
added. Its [prepared repeat](../../benchmarks/WARM-PREPARED-CONFIRM.md) supplies
independent quiet confirmation for Down16 only. Other resize directions, sizes
and filtered kernels remain open. These native tools and their ignored
static archives are never production build inputs.

The [current JPEG decode confirmation](../../benchmarks/WARM-JPEG-DECODE-CONFIRM.md)
reuses the existing frozen native input and worker without a production change
or quality search. Its twelve-sample conservative upper is below 2x; independent
quiet confirmation remains open.

## Warm bilinear resize

The [RGBA16 precision audit](../../benchmarks/BILINEAR16-PRECISION.md) extends
the shared CLI oracle and native adapter to full 16-bit samples. The authored
raster remains unmatched under exact-alpha acceptance, and timing is refused.
The audit uses `tools/WarmReference/Bilinear16Audit/Audit.csproj`; its native
refusal/ownership controls are in `verify-bilinear16-linux.py`. It introduces
no production algorithm change or performance raster.

`WarmBilinearResize/Down8` covers one 513x259 RGBA8 to 256x129 bilinear resize
on the same original transparent raster as the warm nearest case.

```bash
bash tools/WarmReference/build-bilinear-linux.sh
python3 tools/WarmReference/verify-bilinear-linux.py --cpu 0
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Bilinear -WarmCases Down8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Bilinear -WarmCases Down8 -Cpu 0 -NoBuild
```

The original public-FFmpeg-API adapter times all packed/planar conversion, the
zscale widened triangle kernel with straight-alpha metadata and encoded-space
premultiplication/unpremultiplication, final RGBA conversion, and independent
output pixel acquisition/release. Graph/source/wrappers/geometry contexts are
reused; every call filters the full raster. Managed timing covers complete
public Resize/admission/allocation/materialization/disposal. Startup, IPC, I/O,
hashing, generation and validation are excluded; neither subject caches outputs.
One CPU/filter thread, thread type zero and normal SIMD are required.

Managed pixels match an independent exact rational reference. Native alpha and
pinned CLI/deterministic pixels are exact; native RGB uses the existing filtered
maximum 1 / RMSE 0.25 bound. The single one-level difference is a float rounding
boundary, documented with a full-sample float audit. Fourteen native negative
controls, independent live outputs, counters/source integrity and a focused live
test on both OSes pass. The selected output has no zero-alpha pixels; other alpha
edge cases remain open. No extra source raster or quality search occurs.

The [first screen](../../benchmarks/WARM-BILINEAR-BASELINE.md) reaches about 29.18x.
The [row-access inlining round](../../benchmarks/WARM-ROW-INLINE.md) reduces that
to 27.46x, and [one source-row span per vertical tap](../../benchmarks/WARM-ROW-SPAN.md)
reduces it to 9.08x, the largest measured warm gap at that checkpoint. The
[four-channel SIMD trial](../../benchmarks/WARM-RESIZE-SIMD-REJECTED.md) is discarded
without a longer repeat. [Horizontal kernel-weight reuse](../../benchmarks/WARM-RESIZE-XWEIGHTS.md)
reduces complete resize to 8.65x; [destination-row acquisition](../../benchmarks/WARM-RESIZE-DESTINATION.md)
reduces it to 7.25x, and [vertical kernel-weight reuse](../../benchmarks/WARM-RESIZE-YWEIGHTS.md)
reduces it to 7.07x. A fresh profile selects
[typed-row accessor inlining](../../benchmarks/WARM-TYPED-ROW-INLINE.md), reaching
6.93x. [Preparing horizontal geometry once](../../benchmarks/WARM-RESIZE-GEOMETRY.md)
reduces it to 6.34x, including preparation and admitted workspace allocation.
It retains exact binary64 pixels and direct evaluation under tight memory limits.
Substantial filtering-loop improvements take priority while this gap remains.
The [compact prepared kernel](../../benchmarks/WARM-RESIZE-PREPARED.md) reaches
4.35x, retaining exact pixels and the same admitted geometry/allocation footprint.
It captures local buffer views and specializes sample depth for short support;
wide support and tight memory retain direct evaluation.
The [packed RGBA8 accumulation kernel](../../benchmarks/WARM-RESIZE-PACKED-KERNEL.md)
reduces the same complete operation to 2.89x without added allocations. Bounded
sample loading and packed widening retain separate binary64 multiply/add and
the original tap order; scalar and UInt16 behavior remain unchanged.
The [packed output trial](../../benchmarks/WARM-RESIZE-PACKED-OUTPUT-REJECTED.md)
shows no useful short-screen gain and is restored without a longer repeat.
The [paired output-row trial](../../benchmarks/WARM-RESIZE-PAIRED-ROWS-REJECTED.md)
is slower and discarded after one short screen. Its actual timed-DLL JIT retains
per-tap wide coefficient stack materialization for the next code-shape decision.
The [reformulated paired packed-sample kernel](../../benchmarks/WARM-RESIZE-PAIRED-WEIGHTS.md)
reaches 2.64x, with direct coefficient construction and unchanged allocation
totals. Per-pixel binary64 arithmetic, alpha handling and scalar/UInt16 behavior
remain; unequal row supports and odd tails retain the single-row path.
The [grouped four-tap sample kernel](../../benchmarks/WARM-RESIZE-QUAD-SAMPLES.md)
reduces the same complete operation a further 20.5%, to 2.08x (conservative upper
2.12x), without extra allocations. Two bounded 16-byte reads and four unrolled
taps retain per-pixel arithmetic order. Clamped duplicate offsets and other
counts retain the general paired path. No performance raster or pair is added.
The [packed alpha/coefficient kernel](../../benchmarks/WARM-RESIZE-QUAD-ALPHA.md)
reduces the same operation a further 8.9%, to 1.91x (conservative upper 1.93x),
retaining exact pixels and allocations. Packed preparation replaces repeated
scalar alpha conversions without changing each coefficient's arithmetic order.
This pair clears the numerical target; independent quiet confirmation remains
open.
Run sequentially on the quiet dedicated VM and preserve
source/raw/actual timed DLLs before edits. Other sizes, directions, depths and
bicubic remain unmeasured; independent quiet confirmation remains open. Native
sources/tools/static archives are development-only and never production inputs.

## Warm normal orientation

`WarmNormalOrientation/Whole16` reuses the existing original RGBA16 513x259
raster with Normal metadata. Build the original public-API worker and validate
full channel bytes, source, independent live owners and fourteen rejection controls:

```bash
bash tools/WarmReference/build-normal-orient-linux.sh
python3 tools/WarmReference/verify-normal-orient-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily NormalOrientation -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily NormalOrientation -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily NormalOrientation -WarmCases Whole16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

The public FFmpeg null graph produces a shared view. Every native timed operation
uses `av_frame_get_buffer` and `av_frame_copy` to allocate and materialize an
independently owned raster before release; padded rows need no extra packing
copy. Graph/source/frame wrappers are reused. Managed timing includes complete
AutoOrient, both-buffer admission, every row copy and disposal. Both sides may
reuse released storage; processed outputs are never cached. Every channel byte,
alpha and hidden RGB matches pinned normal/scalar CLI output; held outputs prove
mutation isolation. One CPU/filter thread, normal SIMD and the same existing
raster apply. Preparation validates the actual fixed job with one native
operation before/after and null ratios; frozen prepared children then run under
fresh unchanged quiet guards after build activity settles.

The [row-copy round](../../benchmarks/WARM-NORMAL-ROWS.md) reduces screened mean
98.47%. Its twelve-sample repeat reaches 1.062x, upper 1.0871x, but fails the
post-run I/O-wait guard. Quiet confirmation remains open; later cooldown does
not certify the failed interval. Other material cases remain open.

## Warm counterclockwise quarter turn

`WarmCounterclockwise/Whole16` reuses the original RGBA16 513x259 raster,
producing 259x513. Build the original public-API worker and validate full channel
bytes, source, independent live owners and fourteen rejection controls:

```bash
bash tools/WarmReference/build-counterclockwise-linux.sh
python3 tools/WarmReference/verify-counterclockwise-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Counterclockwise -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Counterclockwise -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily Counterclockwise -WarmCases Whole16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

The public FFmpeg transpose graph uses `dir=cclock:passthrough=none`. Each timed
operation allocates and processes independent output pixels before release.
Graph/source/frame wrappers are reused; padded rows need no extra packing or
materialization copy. Managed timing includes complete Rotate(CounterClockwise90),
both-buffer admission, tile placement and disposal. Every channel byte, alpha
and hidden RGB matches the independent coordinate reference and pinned
normal/scalar CLI output; two held outputs prove mutation isolation. One CPU/
filter thread and normal SIMD apply. Prepared children execute their actual
fixed jobs for validation only before statistical replay under fresh guards.

The [bounded tile round](../../benchmarks/WARM-COUNTERCLOCKWISE-TILES.md) reduces
screened mean 96.06%, with both short-screen guards passing. The independent
twelve-actual repeat reaches 1.043x, upper 1.062x, but fails its post-run load
guard. Quiet certification remains open; later cooldown cannot certify the
failed interval. Other material cases remain open.

## Warm plain transpose

`WarmTranspose/Whole16` reuses the original RGBA16 513x259 raster, producing
259x513. Build the original public-API worker and validate full channel bytes,
source, independent live owners and fourteen rejection controls:

```bash
bash tools/WarmReference/build-transpose-linux.sh
python3 tools/WarmReference/verify-transpose-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Transpose -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Transpose -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily Transpose -WarmCases Whole16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

The public FFmpeg transpose graph uses `dir=cclock_flip:passthrough=none`.
Independent forward placement verifies the axis-swap equation before timing.
Each native operation allocates and processes independent output pixels before
release; graph/source/frame wrappers are reused and padded rows need no extra
packing or materialization copy. Managed timing includes complete
Transpose(mirrored:false), admission, tile placement and disposal. Channel bytes,
alpha and hidden RGB match pinned normal/scalar CLI output; two held outputs
prove mutation isolation. One CPU/filter thread and normal SIMD apply. Prepare
the actual fixed job for validation before replay under fresh quiet guards.

The [bounded tile round](../../benchmarks/WARM-TRANSPOSE-TILES.md) reduces screened
mean 95.84%, with both short-screen guards passing. The twelve-actual repeat
reaches 1.088x, upper 1.093x, but fails its post-run load guard. Quiet certification
remains open; later cooldown cannot certify the failed interval. Mirrored
transpose and other material cases remain open.

## Warm mirrored transpose

`WarmTransverse/Whole16` reuses the original RGBA16 513x259 raster, producing
259x513 with both output axes reversed. Build the original public-API worker and
validate every channel byte, source, independent live owners and fourteen controls:

```bash
bash tools/WarmReference/build-transverse-linux.sh
python3 tools/WarmReference/verify-transverse-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Transverse -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Transverse -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily Transverse -WarmCases Whole16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

The public transpose graph uses `dir=clock_flip:passthrough=none`; full independent
forward placement verifies this mapping before timing. Every native operation
allocates and processes independent pixels before release, using reused graph/
source/frame wrappers and directly exposed padded rows. There is no extra copy.
Managed timing includes complete Transpose(mirrored:true), admission, placement
and disposal. Pinned normal/scalar CLI, exact alpha/hidden RGB, source integrity
and two held outputs are checked. One CPU/filter thread and normal SIMD apply.
Prepare the actual fixed job for validation before statistical replay under fresh
guards, with source and actual child binaries frozen before edits.

The [tile round](../../benchmarks/WARM-TRANSVERSE-TILES.md) reduces screened mean
96.32%. The twelve-actual repeat reaches 1.021x, upper 1.033x, but fails its post-run
load guard. Quiet certification remains open; later cooldown cannot certify the
failed interval. Broader editing and other material cases remain open.

## Warm full-raster Clone

`WarmClone/Whole16` reuses the existing original RGBA16 513x259 raster and the
original null-filter worker used for normal-orientation copying:

```bash
bash tools/WarmReference/build-normal-orient-linux.sh
python3 tools/WarmReference/verify-normal-orient-linux.py --cpu 0 --depth 16
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Clone -WarmCases Whole16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Clone -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily Clone -WarmCases Whole16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

Each native operation materializes the shared null-filter view with
av_frame_get_buffer and av_frame_copy into an independent raster before release.
Graph/source/frame wrappers are reused, with padded rows exposed directly and
no redundant packing copy. Managed timing includes public Clone, both-buffer
admission, every row copy and disposal. Exact channel bytes, alpha, hidden RGB,
source and held outputs are validated along with pinned normal/scalar CLI output.
One CPU/filter thread and normal SIMD apply. Preparation validates the actual
fixed job before fresh quiet guards and immutable statistical replay.

The [Clone round](../../benchmarks/WARM-CLONE-REUSE.md) reduces screened mean
95.21%. Its independent twelve-actual repeat passes both quiet guards at 1.076x,
upper 1.089x, certifying this fixed pair. Other material cases remain open.

## Warm RGBA8 transpose coverage

Use the existing transpose worker and second retained original raster:

```bash
python3 tools/WarmReference/verify-transpose-linux.py --cpu 0 --depth 8
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Transpose -WarmCases Whole8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Transpose -WarmCases Whole8 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily Transpose -WarmCases Whole8 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

The owned-output boundary and validation are identical to the plain-transpose
reference above, with four-byte pixels. The [coverage round](../../benchmarks/WARM-TRANSPOSE8-PARITY.md)
changes no production code and adds no raster or geometry. The independent
repeat passes both quiet guards at 0.991x, upper 1.003x, certifying this fixed pair.
