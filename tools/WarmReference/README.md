# Warm FFmpeg reference

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

## Warm nearest resize

`WarmNearestResize/Down8` covers one 513x259 RGBA8 to 256x129 nearest resize
on the existing original raster. It preserves exact integer center coordinates,
alpha and zero-alpha hidden RGB. No extra source image or quality search occurs.

```bash
bash tools/WarmReference/build-nearest-linux.sh
python3 tools/WarmReference/verify-nearest-linux.py --cpu 0
```

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Nearest -WarmCases Down8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Nearest -WarmCases Down8 -Cpu 0 -NoBuild
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
Other resize directions, sizes, depths and filtered kernels remain unmeasured;
independent quiet confirmation remains open. These native tools and their ignored
static archives are never production build inputs.

The [current JPEG decode confirmation](../../benchmarks/WARM-JPEG-DECODE-CONFIRM.md)
reuses the existing frozen native input and worker without a production change
or quality search. Its twelve-sample conservative upper is below 2x; independent
quiet confirmation remains open.
