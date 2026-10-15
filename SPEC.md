# Lokad.Image contract and implementation status

Target: a dependency-free safe C# library for .NET 10, providing still PNG/JPEG/WebP
codecs and basic image editing. This document distinguishes implemented behavior
from required future functionality.

| Capability | Current status | Required target |
| --- | --- | --- |
| PNG decode | Implemented; full authored profile matrix with exact analytical and FFmpeg RGBA | All legal static color/depth combinations, palette/tRNS, five filters and Adam7 |
| PNG encode | Implemented; exact round trips and live FFmpeg checks | Noninterlaced gray/RGB/gray-alpha/RGBA, 8/16-bit |
| JPEG identify | Implemented; live and authored header checks | Bounded baseline/progressive 8-bit inspection and orientation |
| JPEG pixels | Implemented; authored analytical cases, frozen FFmpeg expectations and live comparisons | Baseline/progressive grayscale/YCbCr decode, restart markers, common subsampling; baseline encode |
| WebP identify | Implemented; live and authored header checks | Bounded simple/extended still-container inspection and orientation |
| WebP lossless pixels | Implemented; exact frozen and live FFmpeg comparisons plus authored transform/entropy cases | VP8L decode/encode with transparency and hidden RGB |
| WebP lossy decode | Implemented; exact frozen YUV/alpha reconstruction and bounded RGB comparisons | VP8 key frames with raw/compressed ALPH |
| WebP lossy encode | Implemented; original output independently decoded by FFmpeg | VP8 output with explicit quality/effort and transparency |
| Exact edits | Implemented; hand-computed permutations and live FFmpeg checks | Crop, flips, quarter turns, transpose, all eight EXIF orientations |
| Resize | Implemented; hand-computed kernel and alpha cases | Exact dimensions, fit, nearest/bilinear/bicubic, alpha-aware filtering |

GIF, APNG and animated WebP are rejected. CMYK/YCCK, arithmetic/lossless/12-bit
JPEG, progressive JPEG encoding, indexed/Adam7 PNG encoding, ICC color management,
metadata round-tripping, drawing and font rendering are deferred.

## Pixels and ownership

`ImageBuffer` owns a tightly packed top-to-bottom RGBA byte array. Channels use
straight alpha. `GetRow8` exposes byte samples; `GetRow16` exposes native-endian
unsigned 16-bit samples. 8-bit images use four bytes/pixel, 16-bit images eight.
Rows have no padding. Dispose drops ownership; callers must cease using
previously returned spans, which safe C# cannot revoke. Released PNG decode,
crop and rotation/flip storage may be reused by a later result. Complete-overwrite
operations reconstruct every output sample before publishing their owner and
dispose private results on failure. Live buffers never share their pixel
storage. New `ImageBuffer.Create` buffers are zero initialized.

Transforms return new owned buffers and preserve precision and source pixels.
Outstanding raw orientation is retained until `AutoOrient` or decode with
`ApplyOrientation=true` applies it and resets it to Normal. Coded and oriented
dimensions are separately exposed by `ImageInfo`. Identification validates
container structure and checksums, but does not validate all compressed pixels.

## Geometry and resizing

Coordinates are integer pixel coordinates, with top-left origin. Strict crop
rectangles must fit. Centered crop uses floor division for leftover odd pixels;
clamped crop preserves requested size and clamps its origin to fit.

Fit takes the smaller dimension ratio, optionally caps it at one, and rounds to
the nearest integer with halves away from zero, with a one-pixel minimum.
Resize samples centers at `(destination + 0.5) * sourceSize / destinationSize - 0.5`.
Nearest selects `floor((2 * destination + 1) * sourceSize / (2 * destinationSize))`
on each axis using integer arithmetic. It preserves all four source samples,
including RGB under zero alpha. Bilinear uses a triangle kernel;
bicubic uses Catmull-Rom (Mitchell-Netravali B=0, C=0.5). Downsampling widens the
kernel by the reduction ratio. Out-of-bounds samples extend the edge pixel.

Filtering operates in encoded sample space with premultiplied-alpha accumulation,
followed by unpremultiplication. Round halves away from zero and clamp to sample
range. Filtered pixels whose rounded alpha is zero have RGB zero; exact permutations and
identity resize preserve hidden RGB. No linear-light or ICC conversion is applied.

Short horizontal support may prepare weights and clamped sample offsets once
per operation, reusing them across output rows. This optional geometry workspace
is admitted conservatively and leaves 4 KiB for exact tiny-alpha arithmetic.
Wide support or tight memory limits retain direct evaluation with the same tap
order, binary64 arithmetic and pixels. Geometry is not cached across calls.
RGBA8/16 bicubic downsampling and enlargement on both axes, on
AVX-512F/DQ and AVX2, may reuse unrounded horizontal
intermediates in sixteen operation-local rows, with up to sixteen taps per axis.
RGBA8/16 width reduction with unchanged height may use the same bounded rows;
its conservative guards preserve original binary64 pixels, including exact-half
rounding, with the same workspace admission and direct fallback.
RGBA8 width enlargement with unchanged height may also reuse these sixteen
rows, preserving the existing arithmetic, guards, workspace and direct fallback.
Interior adjacent four-tap outputs may reuse five source vectors instead of eight,
retaining each output's original tap and multiply/add order.
RGBA8/16 height reduction with unchanged width may use these same bounded rows.
Its integer horizontal centers have weights 0,0,1,0,0; copying decoded exact
products into the cache preserves every unrounded intermediate. RGBA8/16 height
enlargement with unchanged width may use the same identity copy and sixteen-row
cache. Cancellation checks every two copied pixels, work charges, precision guards,
memory admission and direct fallback remain unchanged.
RGBA8/16 downsampling with wider support may use thirty-two operation-local rows
and up to thirty-two taps per axis. Its optional workspace is
1420*outputWidth + 32*sourceWidth + 256 bytes, with 4 KiB retained for exact
low-alpha handling. RGBA8/16 may use sixty-four rows/taps for support exceeding
thirty-two taps. That workspace is 2828*outputWidth + 32*sourceWidth + 512 bytes,
with the same 4 KiB allowance. Conservative byte alpha/color margins widen to
1e-4/4e-3 using the 2^-40*4096 per-unit absolute-error bound. Words use an alpha
margin of 4e-3 and a color quotient bound of 32.04/alphaSum, conservatively
computed from the lower power of two. Margins at least 0.25 retain original
evaluation, including clamped out-of-range samples. Each byte or word pixel
independently selects original-order evaluation when its guards are uncertain;
its neighbor retains depth-specific fast packing when every guard passes.
Managed pixels stay exact. Larger support and tight budgets retain direct evaluation.
RGBA8/16 supports exceeding sixty-four taps may use at most 128 operation-local
rows/taps, with workspace `5644*outputWidth + 32*sourceWidth + 1024` bytes and
the same 4 KiB allowance. The `2^-40*128^2` per-unit absolute-error budget bounds
unrounded alpha, premultiplied color and normalization. For each pixel, quotient
error is bounded by `(EC + abs(C/A)*EA)/(A-EA)` for color and
`(EA + abs(A/N)*EN)/(N-EN)` for alpha; the margins are doubled to cover their
own rounding. Nonpositive lower denominators, uncertain rounding, the existing
normalizer/tiny-alpha guards and tight budgets retain original evaluation.
Bounds use the full sample maximum (255 or 65535), including exact unsigned
premultiplied word products. Additional support uses the bounded path below.
Supports above 128 may select the next power-of-two capacity, bounded at 2048
rows/taps, with the same `2^-40*capacity^2` error budget and quotient guards.
For these larger capacities, row identifiers, row offsets and vertical weights
occupy admitted portions of the existing managed arrays instead of larger stack
tables. Workspace is `(44*capacity+12)*outputWidth + 32*sourceWidth + 16*capacity`
bytes, with the same 4 KiB allowance. Memory admission precedes allocation;
support above 2048, insufficient memory and unsupported hardware retain direct
evaluation. Geometry, tap order, work charges, cancellation and pixels remain.
RGBA8/16 may also reuse these rows when one axis grows while the other shrinks.
That path pads odd cache-row widths by one pixel (512 additional workspace
bytes across sixteen rows), allowing safe indexing of cached two-pixel vectors.
Source premultiplication is exact unsigned integer arithmetic, including the
full UInt16 product range. Conservative sample-depth-specific rounding
guards retain the original binary64 order and exact tiny-alpha handling near
ambiguous samples. Optional workspace is `716*outputWidth + 32*sourceWidth + 128`
bytes, admitted before allocation and released on every exit, leaving 4 KiB for
tiny-alpha arithmetic. Tight budgets, unsupported hardware and other directions
retain the previous path; work charges and bounded cancellation are preserved.
When the full row cache cannot be admitted, RGBA8 reduction on both axes may
reuse six-column strips and at most ten rows/taps per axis on AVX512F/AVX2/AVX/SSE4.1
hardware. This optional path admits 4 KiB before its fixed stack workspace
(3,640 bytes of arrays); it adds no heap workspace or persistent state. A bounded
source window is decoded once per reused row. Two independent output accumulators
and paired quotient guards use the same conservative `2^-40*16^2` absolute-error
budget, with exact unsigned byte premultiplication and separate multiply/add.
Uncertain rounding, small normalization or tiny alpha returns to the complete
original-order operation after the stack frame and admission unwind. Speculative
strips leave work accounting untouched; success charges the complete original tap
cost. Every sample remains exact, with bounded cancellation. Other directions,
supports and smaller budgets retain the existing paths.
RGBA16 reduction may use a separate word specialization of the same six-column,
ten-row strip strategy, with the same 4-KiB admission and fixed arrays. Full
UInt16 premultiplication produces unsigned 32-bit products converted exactly to
binary64. The error budget scales with 65535, including squared color products.
Uncertain ordinary-alpha word pixels may replay the original tap order locally;
paired replay requires normalization at least 0.5 and computed normalized alpha
at least two. Any exact tiny-alpha calculation instead declines the complete
strip operation, releasing its frame and charge before original replay needs
scratch. Odd word tails likewise use original-order evaluation. The byte kernel
is separate and remains unchanged; work, precision and owner contracts are retained.
RGBA8 reduction with support above ten and at most twenty taps per axis may use
a separate three-column/twenty-row strip cache on the same hardware. It admits
4 KiB for 4,056 bytes of fixed arrays, retaining scalar coefficient preparation
and the `2^-40*20^2` absolute-error budget. Adjacent exact decoded pixels may be
stored as one checked wide vector; fixed strip offsets and a common-tap prefix
remove repeated indexing/count branches without changing either accumulation's
tap order. Bounds, uncertainty, tiny-alpha replay, complete work and cancellation
remain active. Unsupported supports/windows and smaller budgets keep prior paths.
The same wider-support strip strategy has a separate RGBA16 specialization,
retaining 4-KiB admission and 4,056 bytes of fixed arrays. Full unsigned word
products convert exactly to binary64; quotient bounds scale with 65535 and its
square. Paired and single word outputs use guarded unsigned packing. Uncertain
ordinary-alpha pixels may replay their original tap order locally when
normalization is at least 0.5 and computed normalized alpha is at least two.
Tiny-alpha evaluation instead unwinds the complete strip operation before
original replay needs exact scratch. Source, ownership, work and precision remain;
existing byte and short-support word kernels stay separate and unchanged.
RGBA8 reduction above twenty and up to thirty-six taps per axis may use a
separate two-column/thirty-six-row strip cache. Its 4-KiB admission covers
3,992 bytes of fixed arrays. Exact byte premultiplication products (at most
65025) and alpha fit UInt16; this compact source-window storage introduces no
pixel quantization. Relative window offsets also fit UInt16, while absolute
source coordinates remain Int32 and checked before narrowing. Paired samples
widen exactly to binary64 and preserve each original tap order. The conservative
`2^-40*36^2` error budget, uncertainty/tiny-alpha unwind, complete work and bounded
cancellation remain. Other supports/windows/hardware retain existing paths.
RGBA16 reduction above twenty and up to thirty-four taps per axis may use a
separate two-column/thirty-four-row strip cache. Its 4-KiB admission covers
4,044 bytes of fixed arrays. Full-range premultiplication products (at most
4294836225) remain UInt32 and widen unsigned to binary64 without quantization.
Checked relative source-window offsets fit UInt16; absolute coordinates remain
Int32. The `2^-40*34^2` error budget scales with 65535 and its square. Paired
and single word outputs retain quotient/rounding guards and unsigned packing;
uncertain ordinary-alpha samples replay the original tap order under the same
normalization/alpha guards. Tiny alpha unwinds the frame and admission before
exact scratch is needed. Complete work, bounded cancellation and independent
owners remain; existing byte and smaller-support word kernels are unchanged.
RGBA16 reduction with support above thirty-four and at most seventy-two taps
per axis may use a separate single-column row cache. Its 4-KiB admission covers
4,032 bytes of fixed arrays. Each selected source row is horizontally filtered
once per output column, without a decoded-source table or intermediate pixel
quantization. Paired full-range unsigned word products widen exactly to binary64;
separate additions preserve horizontal tap order. Checked relative offsets retain
absolute Int32 coordinates. The `2^-40*72^2` budget and depth-scaled quotient
guards preserve original pixels, with guarded local ordinary-alpha replay and
complete tiny-alpha unwind before exact scratch. Work, bounded cancellation and
owner contracts remain. Other supports, directions, hardware and smaller budgets
retain the existing paths.
RGBA8 reduction above thirty-six and up to seventy-two taps per axis may use
a separate byte specialization of that single-column row cache, with the same
4-KiB admission and 4,032 bytes of fixed arrays. Exact byte premultiplication
products are at most 65025 and widen to binary64 without quantization. Separate
horizontal additions, checked relative offsets, the `2^-40*72^2` budget and
byte-depth quotient/rounding guards preserve every original sample. Guarded
ordinary-alpha replay and complete tiny-alpha unwind retain exact handling;
complete work, bounded cancellation and independent owners remain unchanged.
Existing word and smaller-support byte kernels stay separate and unchanged.
For RGBA16 reduction with support above seventy-two and at most 304 taps per
axis, a separate streaming path handles any reduction height in blocks of at
most sixteen output rows, reusing the same bounded arrays. Its
4-KiB admission covers 3,936 bytes of fixed arrays. A selected source row is
horizontally filtered once per output column within each block, then feeds the bounded output
accumulators. Increasing unclamped vertical coordinates preserve each output's
original tap order, including separate contributions from duplicated clamped
edge rows; only the last horizontal result is retained. Exact unsigned word
products, checked relative offsets/full absolute Int32 coordinates and the
depth-scaled `2^-40*304^2` quotient guards preserve every sample. Uncertain
ordinary-alpha pixels may replay the original order; tiny alpha unwinds before
exact scratch. Complete work, bounded cancellation and independent owners remain.
No heap workspace, full support cache or output cache is added. Other supports,
directions, hardware and budgets retain the existing paths.
RGBA8 reduction with support above seventy-two and at most 304 taps per axis
may use a separate byte streaming specialization that handles any reduction
height in blocks of at most sixteen output rows. It reuses the same bounded arrays,
with the same 4-KiB admission and 3,936 bytes of
fixed arrays. Exact byte products widen to binary64, and byte-depth quotient
guards use the same `2^-40*304^2` budget. Original tap order and separate clamped
edge contributions, guarded ordinary replay, complete tiny-alpha unwind, work,
cancellation and owner contracts remain. Existing word and smaller-support byte
kernels stay separate and unchanged; no heap workspace or output cache is added.
RGBA16 reductions beyond 304 and at most 544 taps per axis may use a separate
packed-coefficient streaming fallback. It handles any reduction height in blocks
of at most eight output rows, preserving global output coordinates and each
output's complete original tap order. The same bounded arrays are reused. Fixed
arrays occupy 3,776 bytes within a 4-KiB admission. Each original binary64
horizontal coefficient keeps its sign, exponent and 36 fraction bits in six
bytes; the discarded absolute error is measured before filtering. Products and
accumulators remain unrounded binary64. With horizontal/vertical coefficient
L1 sums `Lx/Ly` and measured horizontal loss `Qx`, the absolute unit bound is
`2^-40*544^2*max(1,Lx*Ly/1024) + 2*Qx*Ly`. It conservatively covers original
and separable arithmetic plus coefficient storage loss. Quotient and quarter-
margin guards force original full-coefficient tap-order replay whenever
rounding is uncertain; tiny alpha unwinds before exact scratch. Returned pixels
retain the contract, including independently certified rational-half behavior.
Clamped-edge order, full unsigned products, absolute coordinates, work,
cancellation and independent owners remain. No heap/full-support/output cache
is introduced; other directions, supports, heights, hardware and smaller budgets
retain prior paths.
RGBA8 may use a separate packed-coefficient streaming fallback for the same
above-304/up-to-544 support. It also handles any reduction height in blocks of
at most eight output rows, reusing the same bounded arrays. Its 3,776-byte fixed arrays,
six-byte coefficients, measured discarded error and L1-scaled arithmetic bound
match the word strategy, with byte-scaled quotient guards and exact byte products.
Uncertainty recomputes original full coefficients and tap order; tiny alpha
unwinds before exact scratch. Certified rational halves retain original rounding,
with complete work, cancellation, source and independent owner contracts.
No heap/full-support/output cache is added; other geometry, hardware and budgets
retain prior paths.
RGBA8/16 reductions exceeding 544 taps on either axis may use separate
generated-coefficient streaming fallbacks. Each processes any reduction height in
blocks of at most eight output rows, reusing the same bounded arrays for each
block. Each output retains its complete
original tap order and normalization; no intermediate pixel is stored. It stores no
coefficient table; eight-lane SIMD evaluates the original binary64 Catmull-Rom
operations on demand. A preparatory scan checks every generated coefficient bit
against the original scalar kernel and computes normalization/L1 sums; any
mismatch declines the path. Fixed arrays occupy 512 bytes within 4-KiB admission.
For admitted horizontal/vertical tap bounds `Nx/Ny` and original coefficient
L1 sums `Lx/Ly`, the unit error bound is
`Nx*Ny/2^40*max(1,Lx*Ly/1024)`; the path requires `Nx*Ny <= 2^40`.
It conservatively covers original and separable arithmetic with no coefficient
storage loss. Quotient/quarter-margin guards retain original tap-order replay
for uncertainty; tiny alpha unwinds before exact scratch. Horizontal preparation
and filtering check cancellation every 256 taps. Full unsigned products, checked
loads, complete work, absolute coordinates and independent owners remain.
No heap/full-support/output cache is added; other directions, heights, hardware
and budgets retain prior paths.
RGBA16 enlargement on one or both axes may use a separate eight-column/five-row strip
cache when the full row cache cannot be admitted. Its optional 4-KiB admission
covers 2,416 bytes of fixed arrays. A bounded source window is decoded once per
reused row; exact unsigned word products widen to binary64 without quantization.
Each horizontal/vertical accumulation retains tap order. Paired and single word
outputs use the conservative `2^-40*16^2` budget and depth-scaled quotient guards.
Uncertain ordinary or negative-alpha samples replay the original order locally;
tiny-alpha pixels use an impossible completed-pixel marker in the private output.
After the non-inlined strip frame returns and releases its admission, a timed
scan replays only marked pixels with original color arithmetic and exact rational
alpha scratch, charging its additional work. No marker reaches a returned image.
Complete work, bounded cancellation and independent owners remain. No heap
workspace or persistent state is added; existing byte and reduction kernels stay
unchanged. Other directions, hardware and smaller budgets retain prior paths.
RGBA8 enlargement on one or both axes has a separate byte specialization of the
eight-column/five-row cache, with the same 4-KiB admission and 2,416 bytes of
fixed arrays. Exact byte products, original tap order and byte-depth quotient
guards use the same `2^-40*16^2` budget. Ordinary/negative-alpha uncertainty
replays locally. Tiny-alpha pixels are marked only in the private unfinished
output and resolved after a non-inlined core frame returns and releases its
admission. The marker cannot occur in completed resize pixels because zero alpha
requires zero RGB. The timed final scan replays original color arithmetic and
the existing exact rational alpha rounding, including its additional work charge,
with the full 4-KiB scratch available. No marker reaches a returned image.
Memory/work limits, cancellation and independent owners remain; no extra heap
workspace, persistent state or output cache is introduced. Existing word and
smaller-support byte kernels remain unchanged.
RGBA16 width enlargement combined with height reduction may prefer vertical-first
filtering when horizontal support is at most five taps and vertical support at
most ten. The same path may preserve width while reducing height when the existing
vertical row cache cannot fit; that cache remains preferred when feasible.
Four output rows reuse five source-column slots; 2,016 bytes of fixed
arrays fit an admitted 4-KiB workspace. Exact unsigned word products and unrounded
binary64 vertical results feed horizontal filtering without pixel quantization.
The original coefficient values and each pass's tap order remain; uncertainty
replays the original horizontal-within-vertical order. The established
`2^-40*16^2` bounds use word alpha margin 1e-4 and color margin
`2.0001 / lower-power-of-two(alphaSum)`, with normalizer/alpha/quarter-margin
guards and exact tiny-alpha unwind. Staged paired packing reuses decoded storage
before separate checked stores to the two output rows. Complete work,
cancellation and independent owners remain. No heap workspace, persistent state
or output cache is added; other directions/supports/hardware and smaller budgets
retain prior paths.
RGBA8 may prefer a separate vertical-first specialization for the same
width-growth/height-reduction supports and 2,016-byte fixed arrays in 4-KiB
admission. It may also preserve width when the existing vertical row cache is
unavailable, retaining that faster cache when feasible. Exact byte products
(at most 65,025) widen to unrounded binary64;
the same `2^-40*16^2` bounds permit alpha margin 1e-6 and color margin
`1e-4 / lower-power-of-two(alphaSum)`, retaining normalizer/alpha/quarter-margin
guards and original-order replay for uncertainty. Tiny alpha is marked only in
the private unfinished output, using the impossible completed zero-alpha/nonzero
RGB combination. The noninlined frame returns and releases its workspace before
a timed final scan replays original color arithmetic and exact rational alpha,
including additional work. No marker escapes; other geometry, budgets and
hardware retain prior paths. No heap workspace, persistent state or output
cache is introduced.
RGBA16 width reduction combined with preserved or enlarged height may use a separate
horizontal-first fallback when horizontal support is at most ten taps and
vertical support at most five. The existing row cache remains preferred when
admitted. Eight output columns reuse five filtered source
rows; 32 decoded source-column slots and the other fixed arrays occupy 3,440
bytes within a 4-KiB admission. Full unsigned word products widen exactly to
unrounded binary64. The `2^-40*16^2` quotient bounds and guarded packing retain
original-order replay for uncertain ordinary alpha and frame unwind before
exact tiny-alpha scratch. Each complete operation charges the original tap
work; cancellation, source and independent owner contracts remain. Other
directions/supports/hardware and smaller budgets retain prior paths. No heap
workspace, persistent state or output cache is introduced.
RGBA8 may use a separate horizontal-first fallback for the same width-reduction/
height-preservation-or-enlargement supports and 3,440-byte fixed arrays within
4-KiB admission. The existing row cache remains preferred when admitted.
Exact byte products widen to unrounded binary64; the `2^-40*16^2` guards use
alpha margin 1e-6 and color margin `1e-4 / lower-power-of-two(alphaSum)`.
Uncertainty replays the original tap order. Tiny alpha uses a private deferred
marker, then a noninlined frame unwind and timed exact replay with additional
work and the full scratch allowance. No marker is returned. Other geometry,
hardware and budgets retain prior paths; no heap workspace, persistent state
or output cache is introduced.
When both supports are short, a byte/UInt16-specialized kernel captures local
buffer views and prepares vertical weights/row offsets in bounded stack storage.
It retains the same per-pixel tap order, alpha handling and per-row work charges,
with bounded cancellation and memory admission.
On supported hardware, this kernel loads RGBA8 samples with bounded four-byte
reads and widens them into independent binary64 RGB/alpha accumulation lanes.
Multiplication and addition remain separate, with the original tap order and
exact pixels. Scalar and UInt16 filtering retain their existing behavior.
On AVX512F/AVX2 hardware, matching short row supports may filter two independent
RGBA8 output rows together, sharing horizontal geometry. Bounded sample reads
and direct weight broadcasts preserve each pixel's arithmetic order. Paired
vertical geometry adds 192 bytes of fixed stack storage and no heap workspace.
Unequal supports and odd tails retain the single-row path.
For exactly four consecutive horizontal sample offsets, the paired path may
read one bounded 16-byte group per source row and interleave independent sample
pairs. Four unrolled taps preserve the same binary64 arithmetic order and pixels,
without new workspace. Duplicate clamped offsets and other counts retain the
general paired path.
The four-tap path may prepare weights and alpha coefficients with packed
binary64 arithmetic, preserving wy*wx then *alpha and consuming coefficients
in the original tap order. Inactive coefficients only add zero; output pixels
and admission remain unchanged.

For RGBA8 bilinear enlargement on both axes, AVX512F/AVX512DQ hardware may
filter four output pixels together using exact integer triangle coefficients.
Existing unused geometry slots hold fractional numerators and bounded source
offsets; no extra workspace or pixel cache is introduced. Premultiplied integer
sums remain below 2^47 under output-array admission. Near a quantization boundary
or tiny alpha, the original binary64 calculation and exact-alpha work/memory
checks remain active, preserving established pixels and rounding. Other filters,
mixed scaling and unsupported hardware retain their existing paths.

RGBA16 bilinear enlargement may use the same four-output strategy with exact
64-bit horizontal intermediates, reusing at most four operation-local rows.
Its optional workspace is `128 * outputWidth + 64` bytes and leaves room for
exact tiny-alpha scratch; tighter budgets retain the existing paired-row path.
Unsigned full-word premultiplication and output-array admission keep integer
sums below 2^62. A conservative precision bound covers final binary64 conversion,
with original rounding near ambiguous boundaries. Intermediates are never rounded
to pixels or reused across operations, and admission is released on every exit.

An original development-only reference uses exact integer kernel weights and
`BigInteger` separable accumulation, with no production resize helpers. Public
tests cover both depths, transparent/opaque rasters, up/down/mixed scaling,
one-pixel output and identity. Nearest and identity are exact; filtered values
allow one sample of binary64 rounding disagreement at exact rational halves.
RGBA16 CLI controls preserve full sample precision. On the authored filtered
case, the native float path can differ by one alpha sample near rounding
boundaries while managed output matches the rational reference. The
[precision audit](benchmarks/BILINEAR16-PRECISION.md) records this limitation
under the original exact-alpha gate. The native-only bilinear comparison bound
below subsequently permits this rounding difference; managed pixels remain exact.
Live nearest coordinate-equation controls also cover original 1025/2049-wide
sources at both depths in both directions, including alpha and hidden RGB.
Fractional-support edges and tiny positive alpha separately require zero RGB
whenever the final alpha rounds to zero.
When accumulated alpha lies between zero and one sample, exact integer kernel
weights resolve its final rounding. An exact half therefore becomes alpha one
and retains its visible RGB. This path admits 4 KiB of conservative arithmetic
scratch, charges additional bounded work and checks cancellation; it adds no
coefficient arrays or runtime package dependency. Other filtered channels retain
binary64 accumulation and the original rounding convention.

A live RGBA8 bilinear 513x259-to-256x129 profile checks the same original
transparent raster against FFmpeg's widened triangle kernel with encoded-space
premultiplied-alpha filtering. Managed output matches the exact rational reference;
native alpha is exact and native RGB uses the existing filtered maximum error 1 /
RMSE 0.25 comparison bound. This establishes that one native profile only; other
sizes, depths, bicubic and tiny-alpha native comparisons remain open.

The original RGBA16 bilinear comparison admits a native-only alpha maximum
error of one full 16-bit sample and RMSE at most 0.25, alongside the same RGB
bounds. FFmpeg's float stages differ near half-sample rounding boundaries.
Lokad.Image must still match the exact rational reference on that raster;
this comparison tolerance does not change its pixel contract. RGBA8 native
alpha remains exact, and native zero-alpha RGB must remain zero at both depths.
On AVX-512-capable CPUs, short-support RGBA16 filtering processes two output
rows in independent SIMD lanes, with the original tap and multiply/add order.
Other CPUs and an unpaired final row retain scalar filtering. Memory admission,
per-row work charges and exact tiny-alpha rounding remain unchanged.
RGBA16 tap alpha extraction and coefficient application stay in SIMD lanes;
full samples are widened once, with no intermediate image or added scratch.
RGBA16 stores normalize and round four channels together. Half comparisons use
the fractional remainder, preserving adjacent binary64 values on either side
of a half; final channels saturate to the full 16-bit range.

## Formats and metadata

PNG follows the [W3C specification](https://www.w3.org/TR/png/). Validate signature,
chunk lengths/names/order/CRCs, image geometry, transparency and decoded pixel
count. An owned bounded Deflate framing walk validates final blocks, stored
lengths, fixed/dynamic prefix trees, repeats, backward distances, the declared
zlib window and the exact filtered-raster byte count before BCL inflation.
Concatenated IDAT bytes may reuse one released exact-size private array, retained
up to 1 MiB independently of pixel storage. Every byte is copied before framing;
the lease ends after the inflater and streams are disposed, including failures.
Input admission remains exact and live leases never share storage.
Noninterlaced, noncancelable decoding may admit 8 KiB of buffered BCL output when
all remaining row work and memory fit. Tight budgets retain unbuffered reads.
A speculative inflater error reopens the owned input, skips only delivered bytes
and resumes original reads, preserving row/filter/palette failure ordering.
Validate the zlib header and Adler checksum at the actual stream boundary;
ignore unused trailing IDAT bytes as permitted by PNG section 11.2.3. Future
reserved chunk bits follow unknown-chunk handling: unknown critical chunks fail,
and unknown ancillary chunks are skipped. Recognized EXIF must precede IDAT.
Unused ancillary payloads are skipped without decompression. Decoder scratch and
compressed buffers cease admission before requested orientation. The encoder uses
filter None, splits IDAT output into bounded chunks, and strips metadata. Selecting
gray requires equal RGB channels; selecting a format without alpha requires
opaque pixels. No implicit grayscale conversion or alpha removal occurs.

RGBA8 encoding copies complete RGBA rows in groups of at most 4096 pixels, with
cancellation checked before each group. Encoded bytes, hidden RGB/alpha, work
charges, memory admission and row workspace remain unchanged across compression
policies. Other color types and 16-bit sample packing retain their validation.

The original corpus covers every legal static color/depth profile at both interlace
settings, all five filters, packed padding, partial palette alpha and hidden RGB.
Dense RGB8 rows without tRNS expand directly into RGBA8 with opaque alpha.
Bounded 24-byte AVX2 groups produce eight pixels; scalar tails and other hardware
retain every sample. Dense Adam7 passes qualify, while sparse paths
retain their mapping. No workspace or admission change is introduced;
cancellation is checked at most every 4096 pixels.
Dense RGB8 rows with a six-byte tRNS key use bounded eight-pixel expansion and
compare all three complete 16-bit keys. Exact matches receive alpha zero while
retaining their RGB samples; any key channel above 255 never matches. Scalar
tails and other hardware preserve the same mapping. Dense Adam7 passes qualify;
sparse paths, admission, work and cancellation cadence remain unchanged, with
no additional workspace.
Dense packed gray1/2/4 rows without tRNS expand directly into RGBA8. Bounded
AVX2 groups read one/two/four bytes for eight pixels, extract MSB-first samples,
apply exact scales 255/85/17 and insert opaque alpha. Scalar tails ignore unused
padding. Dense Adam7 passes qualify; sparse/transparency paths retain their
mapping. No workspace/admission change is introduced; cancellation is checked
at most every 4096 pixels.
Dense RGB16 rows without tRNS expand directly into native-endian RGBA16 with
opaque alpha. On AVX2 hardware, bounded 24-byte groups become four output pixels
through byte shuffles; scalar tails and other hardware preserve full samples.
This also covers dense Adam7 passes. Sparse passes and transparency retain their
existing mapping. Work and memory admission are unchanged; expansion checks
cancellation at most every 4096 pixels and adds no workspace.
Dense gray8 rows without tRNS expand directly to equal RGB samples and opaque
alpha. Bounded AVX2 groups process eight samples; tails and other hardware use
scalar expansion. Dense Adam7 passes also qualify; sparse paths, work and memory
admission remain unchanged, with no additional workspace.
Dense gray8 rows with a two-byte tRNS key use the same bounded eight-sample
expansion with an exact full-key comparison. Matching pixels retain their gray
RGB and receive alpha zero; other pixels are opaque. Keys beyond the 8-bit sample
range never match. Scalar tails, other hardware and cancellation at most every
4096 pixels preserve the existing mapping without additional workspace.
Dense gray16 rows without tRNS expand directly into native-endian RGBA16,
retaining the full gray sample in RGB and inserting alpha 65535. Bounded
eight-byte AVX2 groups produce four pixels; scalar tails and other hardware
preserve all samples. Dense Adam7 passes qualify; sparse/transparency paths
retain their mapping. Workspace/admission are unchanged and cancellation is
checked at most every 4096 pixels.
Dense gray-alpha8 rows expand directly to equal RGB samples and retain alpha,
including hidden RGB. Bounded AVX2 byte shuffles process eight pixels; scalar
tails and other hardware preserve all samples. Dense Adam7 passes qualify,
while sparse passes retain their mapping. No workspace or admission change is
introduced; cancellation is checked at most every 4096 pixels.
Dense gray-alpha16 rows expand directly into native-endian RGBA16, retaining
every gray/alpha sample and hidden RGB. Bounded 16-byte AVX2 groups produce four
pixels; scalar tails and other hardware preserve full samples. Dense Adam7 passes
qualify; sparse mapping stays unchanged. No workspace/admission change is
introduced and cancellation is checked at most every 4096 pixels.
Dense palette8 rows expand directly from the existing RGB palette/tRNS storage
into RGBA8, preserving hidden RGB and defaulting missing alpha entries to 255.
Each palette index is validated before its output pixel is written. Dense Adam7
passes qualify; sparse mapping stays unchanged. Dense packed palette1/2/4 rows
also expand directly. Full 16-entry palette4 rows with absent/full tRNS use bounded
eight-byte SSSE3 groups to produce sixteen pixels; partial palettes/alpha, tails
and other hardware retain scalar validation. Unused padding bits are ignored.
No palette workspace or admission change is introduced; cancellation is checked
at most every 4096 pixels.
The registered FFmpeg 8.1 decoder ignores tRNS alpha for 1/2/4-bit grayscale.
For these cases the reference pipeline applies the declared transparent gray key
through FFmpeg's [colorkey filter](https://ffmpeg.org/ffmpeg-filters.html#colorkey),
with similarity 0.01 and blend zero. Packed sample expansion is injective and
adjacent gray levels differ by at least 17/255, so only the declared key is selected.
All 72 authored profiles require exact native and analytical RGBA. Unfiltered
native output is retained separately; 44 live cases check every transparent key
at both interlace settings without a pixel tolerance.

JPEG follows [T.81](https://www.itu.int/rec/T-REC-T.81/en). Decode 8-bit Huffman
baseline sequential and progressive grayscale/YCbCr at 4:4:4, 4:2:2 and 4:2:0,
including equivalent sampling-factor representations. Chroma samples use centered
bilinear expansion with edge extension. Convert full-range YCbCr using
R=Y+1.402Cr, G=Y-0.344136286Cb-0.714136286Cr, B=Y+1.772Cb (Cb/Cr centered at 128).
IDCT uses the direct mathematical transform, with samples rounded and clipped
before chroma expansion. The original T.81 basis is frozen as binary64 values;
this keeps coefficient and pixel rounding independent of platform `Math.Cos`
differences. RGB component identifiers or Adobe transform 0 select
direct RGB planes. Decode produces opaque RGBA8; no ICC conversion is applied.
On AVX-512/VL, horizontal/vertical passes are accepted only when a conservative
rounding certificate proves the original byte. Uncertain rows and extreme blocks
use the original direct evaluation; other processors retain that evaluation.
The [inverse rounding proof](docs/jpeg-inverse-rounding.md) defines the bound.

Progressive scans retain coefficients in spectral order. After all scans and
component DC progression are validated, reconstruction reuses the already
admitted approximation array to map each block to natural order. On AVX2/BMI2,
correction traversal can discover one nonzero-position mask per refinement block,
reuse it across symbols and update it only when a coefficient is inserted. Zero
runs count only zero positions; grouped bits visit nonzero positions in spectral
order while preserving insertion boundaries and signs. Each correction retains its original
logical work charge. Cancellable tokens, incomplete entropy, tight work budgets
and unsupported hardware retain scalar correction reads and failure precedence.

The JPEG encoder emits baseline Huffman JFIF with selectable grayscale, 4:4:4,
4:2:2 or 4:2:0 sampling (default 4:2:0). Grayscale explicitly converts RGB to
luma. Chroma downsampling averages each sampling cell and extends edge pixels.
Quality 1..100 (default 85) controls an original frequency-weighted quantization
formula: luma basis `1+2(u+v)+uv`, chroma basis `2+3(u+v)+uv`, scaled by
`quality<50 ? 5000/quality : 200-2*quality`, then
`clamp((basis*scale+50)/100,1,255)` with integer division. Quality 100 still loses
information through color conversion, sampling and coefficient rounding. Huffman
tables use independently implemented canonical coding. Large encodes to an
ordinary MemoryStream with an explicit background, a noncancellable token and
sufficient work/memory admission use per-image adaptive tables limited to 16
bits. Other calls retain the original fixed-length tables. This selection does
not change quantized coefficients or decoded samples; encoded bytes may differ
with stream, cancellation or resource options. Optional restart intervals count
MCUs and reset DC predictors.
The forward transform evaluates independent coefficients with safe AVX/AVX-512
when available, preserving the frozen binary64 basis, separate multiplications
and accumulation order. Other processors use the original scalar calculation.
Encoding on AVX-512 can combine a separable transform with quantization when a
conservative [rounding certificate](docs/jpeg-forward-quantization.md) proves the
original quantized integer. Uncertain coefficients use the original direct
evaluation. Encoded bytes, standalone coefficient bits and work admission remain
unchanged; unsupported hardware uses the original transform and quantization.

Adaptive coding requires at least 256 blocks. Sampling and transforms run once;
an admitted packed-event buffer records DC/AC symbols and amplitudes, including
restart positions. Separate luma/chroma tables reserve the all-ones code. The
buffer is private, leased per thread, and retained only up to 4 MiB; every encode
recomputes its events. Admission includes its full capacity and a 64 KiB table/
scratch allowance. Histogram traversal and table construction charge work.
Lower output quotas retain adaptive coding and enforce every write; combined
symbol writes apply only when conservative output and work bounds are satisfied.

JPEG requires opaque input or an explicit `BackgroundRgb` packed as 0xRRGGBB.
Alpha flattening operates in encoded sample space before 16-to-8-bit conversion,
with one nearest-integer rounding. Output is opaque and strips input metadata.
Both 8-bit and 16-bit RGBA sources are accepted.
Dense one-to-one encoder component rows may use safe SIMD: AVX-512/AVX2 for
RGBA8 and AVX-512F/DQ for RGBA16, including luma in 4:2:0 and 4:2:2. The RGBA16
blend's integer products and sums are exact in binary64; flooring its adjusted
quotient reproduces the original integer normalization for every sample.
Right-edge padding and other hardware retain their existing paths. Sample
arithmetic, work charges, memory admission and encoded bytes are unchanged;
no additional image workspace or converted-pixel cache is introduced.
Dense 2:1 chroma rows may convert sixteen RGBA8 or RGBA16 pixels from one or two source
rows with safe SIMD. Unrounded binary64 component values accumulate in the
original row/pixel order, average, then round once. RGBA16 taps use the same exact
single-round normalization as the full-resolution row kernel. Edges and other
hardware retain scalar sampling; no extra workspace or pixel cache is introduced.
JPEG coefficient planes, Huffman tables, scans and transforms are charged against
operation limits.
Component coefficient storage is released after conversion to its sample plane;
table/sample-plane charges conservatively remain through optional orientation.
Every Huffman redefinition is charged, including unused or replaced tables.
JPEG encoding admits the supplied pixels plus 8192 bytes of fixed workspace
before writing output. The [resource admission notes](docs/resource-admission.md)
describe the reviewed lifetimes and exact boundary tests.

The original JPEG correctness corpus uses maximum RGB-channel error 4 and RGB
RMSE 1.5 against pinned FFmpeg, with exact dimensions and alpha 255. These bounds
cover IDCT and color-rounding differences; they are not encoder quality targets.
FFmpeg reference conversion expands extracted chroma planes by their sampling
factors before cropping odd dimensions and applies explicit full-range BT.601
conversion. Numerical comparison settings are recorded with each frozen fixture.
The original [JPEG quality/size report](tests/Quality/README.md) records 320 cases
across five patterns, all four sampling modes, both source depths and four settings
per encoder. Quantization endpoints, actual sampling factors and once-only alpha
flattening have public API checks. FFmpeg qscale values and Lokad.Image quality
values are independent controls; equal positions in the report do not imply
matched quality, compression efficiency or speed.

WebP follows
[RFC 9649](https://www.rfc-editor.org/rfc/rfc9649.html) and VP8 intra-frame sections
of [RFC 6386](https://www.rfc-editor.org/rfc/rfc6386.html). VP8L decoding implements
all four transforms, all fourteen predictor modes, packed palettes, spatial prefix
groups, canonical prefix codes, color caches and overlapping backward references.
Simple and extended still containers are accepted, including EXIF/XMP before or
after the pixel chunk. Alpha samples and hidden RGB are preserved even when the
header's alpha hint is unset. Reserved and future VP8X fields are ignored as
required by RFC 9649; short headers and impossible canvas products are rejected.
VP8L admits auxiliary rasters, cache/group/tree state and overlapping palette
expansion before allocation. Predictor-mode validation and entropy group discovery
charge every atlas entry, checking work/cancellation every 1024 entries. Decoder
workspace charges are released before optional orientation. Exact memory/work
boundaries are recorded in [resource admission](docs/resource-admission.md).
Canonical Huffman lookup may resolve an entire symbol using local bit-reader
state and the existing tables. It charges the same logical bit reads and consumes
no byte beyond the symbol. Tight work budgets and truncated inputs retain the
per-bit path, partial consumption and original error precedence. Table admission
and zero-bit single leaves are unchanged; no additional lookup storage is used.
Output is RGBA8; VP8L dimensions are at most 16384
per side.

VP8 key-frame decoding implements arithmetic partitions, segmentation, all ten
subblock and four large-block prediction modes, coefficient contexts, integer
DCT/Walsh inversion and normal/simple loop filtering. Zero frame-level filter
strength disables the entire filtering stage, including positive segment,
reference and mode adjustments. A nonzero frame level permits the per-macroblock
adjusted strength, clamped to 0..63. Versions 0..3 are accepted;
reserved color spaces fail explicitly. Video display scaling hints do not resize
the coded WebP canvas. Coded dimensions
are at most 16383 per side. The separate ALPH chunk supports raw and headerless
VP8L compression, all four inverse filters, and modulo-256 reconstruction. Reserved
ALPH bits and the informative level-reduction flag do not change decoded samples.
Raw alpha copying checks work/cancellation every 64 KiB. The VP8 parser/mode
charges are released before alpha decoding; decoded YUV/alpha charges remain
through RGBA conversion and are released before optional orientation.
The required probability and quantization constants come from RFC 6386's narrative
tables; decoding algorithms are independently written.

VP8 reconstruction matches frozen FFmpeg Y/U/V planes and alpha exactly. Output is
RGBA8 using centered bilinear chroma expansion with edge extension, rounded to
8-bit chroma samples before explicit limited-range BT.601 conversion. Color
conversion uses Y'=(Y-16)*255/219, U'=U-128, V'=V-128;
R=Y'+1.596026786V', G=Y'-0.391762290U'-0.812967647V', B=Y'+2.017232143U',
rounded and clamped. The original corpus requires RGB maximum error 4 and RMSE 1.5
against FFmpeg with the same expansion/range settings, exact geometry and exact
alpha. These bounds cover scaler/matrix rounding; they do not permit errors in
integer reconstruction or alpha.
The FFmpeg oracle saturates through RGBA16 before explicitly reducing to RGBA8.
The registered 8.1 scalar direct-RGBA8 conversion wraps blue for the analytical
YUV=(255,255,128) case; the 16-bit path clips correctly. This conversion choice is
checked against analytical saturated colors and recorded in fixture arguments.
The RGB tolerances remain unchanged. Original authored cases additionally cover
all token partition counts, sparse categories and probability updates, both
segment feature modes, quantizer deltas/clamps and filter adjustment/sharpness
boundaries. Empty partitions and inconsistent sizes are rejected.

`EncodeWebp` defaults to lossless VP8L. RGBA16 input is normalized per channel
with integer `(sample+128)/257`; RGBA8 samples are preserved exactly, including
hidden RGB. The first encoder uses independently generated fixed canonical tables
and repeated-color runs, without transforms, a color cache or general match search.
Encoded size is not yet optimized. `Effort` accepts 0..6 (default 4): zero writes
only literals; other settings permit repeated-color runs up to `64<<effort` pixels.
`Quality` accepts 1..100 but is ignored in lossless mode. VP8L payload length is counted before writing, so nonseekable output
requires no encoded-frame staging buffer. Both passes charge work and require
caller synchronization of mutable input pixels. Unsupported profiles fail
explicitly, without external fallbacks.

`Lossless=false` emits a VP8 key frame with 4:2:0 chroma, a mathematical forward
4x4 DCT and the format's quantizers. `Quality` (default 85) maps to quantizer index
`((100-quality)*127+49)/99` with integer division. Quality 100 is still lossy.
Quantized coefficients round halves away from zero; the forward DC basis preserves
exact constant-block half ties.
Numeric qualities do not promise the same reconstruction or size as another
encoder's quality settings. Limited-range BT.601 source conversion uses
Y=16+(219/255)(0.299R+0.587G+0.114B),
U=128+(224/255)(-0.168735892R-0.331264108G+0.5B), and
V=128+(224/255)(0.5R-0.418687589G-0.081312411B).
Chroma averages each 2x2 source RGB cell in encoded sample space, weighted by
its normalized alpha before matrix conversion and rounding. Fully transparent
cells retain their original unweighted average; opaque and uniform-opacity
cells keep the same result. Luma and exact normalized alpha are unchanged.
Odd and padded edges
extend the last source pixel. Alpha is normalized once and preserved exactly in
a raw unfiltered ALPH chunk. Optional metadata is stripped.

The first VP8 encoder uses B_PRED luma, one token partition, fixed entropy models
and no loop filter. Prediction search minimizes source-plane squared error;
ties choose the first candidate. Effort 0..6 considers respectively 1, 2, 3, 4, 6,
8 or 10 luma modes in format order (DC, true-motion, vertical, horizontal,
left-down, right-down, vertical-right, vertical-left, horizontal-down,
horizontal-up). Effort zero uses DC chroma; other settings consider all four
chroma modes. Reconstruction for subsequent predictions uses the specified
integer inverse transform. Higher effort expands search; it does not guarantee
smaller output or lower final RGB error. Compression efficiency is not yet
optimized. Two bounded arithmetic partitions are staged for nonseekable output;
the first partition has the format's 19-bit length bound. Working memory includes
source/reconstructed planes, mode/context arrays, alpha and partition growth.
Original quality/size evidence and its limits are recorded in
[tests/Quality](tests/Quality/README.md).

EXIF parsing reads only the IFD0 orientation field, bounds every accessed offset,
and never follows unrelated tags or directory links. `MaxContainerItems` also
bounds IFD0 entry count; a structurally valid directory over that limit raises
`ImageLimitException`. A truncated directory raises `ImageDataException`.
Optional metadata is stripped
on PNG/JPEG/WebP output. Container inspection may read the entire supplied encoded input,
but never allocates a decoded pixel plane.

Original public-API and live FFmpeg controls cover every orientation at both
TIFF byte orders for PNG8/PNG16, baseline/progressive 4:2:0 JPEG and lossless/lossy
WebP with alpha. Lossless orientation retains exact samples and hidden RGB.
The native control uses an explicit RGB permutation after the declared color
conversion, with implicit autorotation disabled; it validates the geometry and
decoder agreement rather than FFmpeg's metadata dispatch.

## Limits, failures and streams

Default per-operation limits are 32768 pixels/side, 32 million pixels, 64 MiB input
and encoded output, 512 MiB admitted working memory, 4 MiB optional metadata,
100000 container items and two billion charged work units. Codec-specific limits
apply in addition. All limits are positive immutable options and may be lowered.
Work units are deterministic admission accounting, not elapsed-time guarantees.

Working-memory admission includes supplied pixels/encoded input, result pixels,
codec buffers and conservative allowances for BCL compression. This is an
allocation policy, not a bound on total process RSS or .NET GC heap retention.
Limits and cancellation are checked before allocation and within long loops.
PNG encoding admits a 1 MiB BCL compression allowance, one packed row and a
64 KiB IDAT buffer in addition to the supplied pixels, before writing its signature.
PNG decoding retains its separate 256 KiB inflater allowance. Geometry limits
apply to new buffers and codec input; transforms count an existing larger source
in working memory even when their result fits lower geometry limits. Exact
mapping checks work/cancellation every 1024 output pixels, and cloning every
16 KiB of row data, including when the caller raises dimension limits.
Horizontal flip reverses whole RGBA8/RGBA16 pixels with safe span-based SIMD
when supported, with a scalar fallback and the same admission checkpoints.
Vertical flip copies complete rows in reversed placement at both depths, with
work/cancellation checks every 1024 pixels. Half turns reverse complete packed
pixels with safe SIMD and a scalar fallback at the same bounded work intervals.
Crop, vertical flip and half turns use the
existing bounded reusable storage only for results that overwrite every sample;
partial failures dispose their private result before publication.
Clockwise and counterclockwise quarter turns place complete RGBA8/RGBA16 pixels
in bounded 32x32 tiles with the same reusable storage policy, both-buffer admission and at most
1024 pixels between work/cancellation checks. Partial tiles preserve exact
placement and precision. Plain and mirrored transpose use the same complete-pixel
tile, allocation, admission and bounded-work policy, with exact axis swapping
and reversal of both axes when mirrored.
Normal-orientation AutoOrient copies complete RGBA8/RGBA16 rows into independent
storage with the same complete-overwrite policy and at most 1024 pixels between
work/cancellation checks. It retains every channel byte and resets orientation
to Normal; source buffers and other mapping kernels are unchanged.
Clone uses the same admitted complete-overwrite storage policy, preserving every
channel byte, source orientation and independent ownership. Its original row
copying, sample work count and at most 16384-byte cancellation intervals are
unchanged. Private partial results are disposed on failure; zero-initialized
Create buffers retain their separate allocation policy.
See [resource admission](docs/resource-admission.md) for review evidence and scope.

Malformed data raises `ImageDataException`, unsupported features raise
`UnsupportedImageException`, and exhausted limits raise `ImageLimitException`.
Cancellation uses `OperationCanceledException`; invalid public arguments use
standard argument exceptions. Programming errors are not translated into corrupt
input errors. Mutable buffers require caller synchronization; independent calls
share no mutable operation settings.

Supplied streams remain open. Nonseekable input uses bounded buffering. Stream
encoding can leave partial output on failure: callers needing atomic publication
must stage output themselves. No filesystem or process capabilities are used by
production code.
