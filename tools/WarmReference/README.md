# Warm FFmpeg reference

## Prepared confirmation

Compile, generate the BenchmarkDotNet child and validate it before the quiet
interval. For a new case, prepare the fixed confirmation job on the VM:

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily Crop -WarmCases Fixed16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarmConfirm
```

Preparation generates and executes the actual twelve-iteration WarmConfirm job
with one native operation before and after for execution/output validation.
Its receipt is explicitly validation-only, with a null parity ratio. Retain
the generated files; allow preparation activity to settle before a fresh guard.
This supplies a compatible child without compiling inside the scored window.
A retained compatible `WarmConfirm` child can run directly without
rebuilding the host. Verify its generated entry selects the declared method,
case and job, and smoke-check its current assemblies first. Keep the generated
files ignored; freeze the actual child, benchmark/library DLLs, runtime config,
owned source inventory and native identity. Never reuse a child for a different
method or case without verifying its generated dispatch.

On the dedicated VM, a controller sets these values and checks the unchanged
quiet guard immediately before and after the process:

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
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -Cpu 0 -NoBuild
# Iterate one measured gap without adding images or unrelated cases.
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode16 -Cpu 0 -NoBuild
```

The three pairs are RGBA8 encode, RGBA8 decode and RGBA16 decode on the same two
original 513x259 rasters as horizontal flip. Both decoders consume identical
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

```bash
bash tools/WarmReference/build-jpeg-decode-linux.sh
python3 tools/WarmReference/verify-jpeg-decode-linux.py --cpu 0
```

After a Release benchmark build and owned source inventory:

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily JpegDecode -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily JpegDecode -WarmCases NativeDecode8 -Cpu 0 -NoBuild
```

Each native interval resets decoder state, decodes all coefficients/pixels,
converts full-range BT.601 to RGBA8, allocates independent output, and releases
it. Codec/conversion/input packet/frame wrappers are reused. Padded owned
RGBA rows need no tight-row copy inside timing. Managed public Decode,
allocation and disposal are included. Startup, I/O, IPC and checks are excluded.
Two simultaneously live outputs must have independent storage; mutation of
one leaves the other intact. Repeated output pixels and encoded source bytes
must remain unchanged. Fourteen invalid protocol/argument/affinity controls
reuse this JPEG. Setup verifies the native pixels exactly against the pinned
public FFmpeg CLI with the same conversion flags and normal SIMD; managed/native
agreement retains RGB maximum 4/RMSE 1.5 and exact opaque alpha. Decoder timing
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
added. Other resize directions, sizes and filtered kernels remain unmeasured;
independent quiet confirmation remains open. These native tools and their ignored
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
