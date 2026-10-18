# Lokad.Image contract and implementation status

Target: a dependency-free safe C# library for .NET 10, providing still PNG/JPEG/WebP
codecs and basic image editing. This document distinguishes implemented behavior
from required future functionality.

| Capability | Current status | Required target |
| --- | --- | --- |
| PNG decode | Implemented; full authored profile matrix with exact analytical and FFmpeg RGBA | All legal static color/depth combinations, palette/tRNS, five filters and Adam7 |
| PNG encode | Implemented; exact round trips and live FFmpeg checks | Noninterlaced gray/RGB/gray-alpha/RGBA, 8/16-bit |
| JPEG identify | Implemented; live and authored header checks | Bounded baseline/progressive 8-bit inspection and orientation |
| JPEG pixels | Implemented; authored analytical cases, frozen FFmpeg expectations and live comparisons | Baseline/progressive gray/RGB/YCbCr/CMYK/YCCK decode, restart markers and required sampling; baseline gray/YCbCr encode |
| WebP identify | Implemented; live and authored header checks | Bounded simple/extended still-container inspection and orientation |
| WebP lossless pixels | Implemented; exact frozen and live FFmpeg comparisons plus authored transform/entropy cases | VP8L decode/encode with transparency and hidden RGB |
| WebP lossy decode | Implemented; exact frozen YUV/alpha reconstruction and bounded RGB comparisons | VP8 key frames with raw/compressed ALPH |
| WebP lossy encode | Implemented; original output independently decoded by FFmpeg | VP8 output with explicit quality/effort and transparency |
| Exact edits | Implemented; hand-computed permutations and live FFmpeg checks | Crop, flips, quarter turns, transpose, all eight EXIF orientations |
| Resize | Implemented; hand-computed kernel and alpha cases | Exact dimensions, fit, nearest/bilinear/bicubic, alpha-aware filtering |
| JPEG caller samples | Gray/RGB/CMYK decode and gray/RGB encode implemented in development | Caller-provided 8-bit samples without mandatory RGBA; explicit source color space/inversion, quality and sampling |
| JPEG compatibility extensions | Implemented in development | CMYK/YCCK and JPEG 4:1:1 (4×1) / 4:1:0 (4×2) sampling |
| Stream extensions | Async Identify/Decode/Encode and fixed-workspace InspectHeaders implemented in development | Async I/O and bounded-memory large-image dimension/orientation inspection; avoid redundant encoded-input and full-frame copies |
| Drawing/compositing | Implemented in development | Alpha compositing, rectangle fills and rectangular pixel copies at 8/16 bits |
| Text | Implemented in development: supplied TrueType loading, outlines, pixel layout/measurement and 8/16-bit antialiased rendering | Supplied TrueType fonts; size/style/color/position, wrapping, line spacing and layout bounds for overflow/fitting |
| Failure details | Implemented in development | Typed exhausted-limit and unsupported-feature categories with recognized codec when known |
| Shared admission | Implemented in development | ImageBudget codec entry points and caller reservations; coarse per-operation reservation, cumulative charged work and independently retained BudgetedImage outputs |
| Compression efficiency | Required ongoing work | WebP lossless sizes and lossy size/quality/time; JPEG comparisons at matched quality or size on representative photographs and text/diagrams |

GIF, APNG and animated WebP are rejected. Arithmetic/lossless/12-bit JPEG,
progressive JPEG encoding, indexed/Adam7 PNG encoding and metadata round-tripping
are deferred. ICC conversion remains opt-in and is not currently implemented;
EXIF reorientation is opt-in. Planned extensions preserve resource limits,
cancellation, pixel precision, caller ownership and independent output lifetimes.

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
Released pixel storage may retain at most two exact-size arrays, with their
combined idle bytes capped at 4 MiB. Only released leases are eligible; active
owners never share storage. An unmatched size discards idle entries. Retention
does not change input/result byte admission, and oversized arrays are not cached.

JPEG decoding may also retain three idle exact-size coefficient arrays, capped
at 4 MiB combined. Every lease is fully zeroed before parsing and returned after
its last use or a decode failure. Live decoders never share a lease. Original
coefficient working-memory and work admission remain unchanged.

`ImageCodec.DecodeJpeg` writes caller-owned interleaved 8-bit grayscale, RGB or CMYK
samples without an RGBA frame or encoded-input copy. Stride zero means packed;
positive stride must cover a row. Padding/suffix bytes are untouched. The supplied
input and complete destination spans count toward working admission and must not
overlap. `ImageBudget.DecodeJpeg` uses the same operation, releasing codec
admission on return; caller storage lifetimes can use an explicit reservation.
Failures may leave a completed pixel prefix; no buffer ownership is transferred.
`JpegDecodeOptions.SourceColorSpace` selects encoded Gray/RGB/YCbCr/CMYK/YCCK interpretation;
Auto preserves component-count, Adobe and RGB-ID inference. RGB output uses the
existing encoded-space color matrix and rounding. Grayscale takes decoded Y for
YCbCr, repeats gray unchanged, or rounds `0.299 R + 0.587 G + 0.114 B` for RGB.
EXIF reorientation is opt-in and writes directly to the oriented destination
layout; returned dimensions/orientation describe those samples. ICC is not applied.

Four-component Auto interprets Adobe APP14 transform 2 as YCCK and transform 0
or an absent marker as CMYK. Transform 1 is unsupported for four components.
Canonical CMYK samples use 0 for no ink and 255 for full ink. YCCK applies the
owned inverse YCbCr matrix to `(255-C,255-M,255-Y)` while keeping K independent.
`CmykInversion.Auto` then inverts all four values for Adobe transform 0/2, leaving
untagged data unchanged; `None` and `Invert` override that convention explicitly.
These defaults accommodate the [documented inversion ambiguity](https://raw.githubusercontent.com/libjpeg-turbo/libjpeg-turbo/main/doc/libjpeg.txt).
RGB uses `(255-C)*(255-K)/255` per channel, with unrounded matrix/black arithmetic
until the final clamp/round; grayscale weights those unrounded RGB values.
This is a device-color approximation with no ICC processing. JPEG K is never
alpha. CMYK output requires four encoded components; CMYK encoding is unsupported.
Four-component frames use the existing sampling-factor and ten-block MCU bounds.

`ImageCodec.EncodeJpeg` and `ImageBudget.EncodeJpeg` also accept borrowed gray/RGB
samples and positive or packed strides. Input is neither changed nor retained.
Gray input always produces one component; RGB input uses `JpegEncodeOptions`
quality, sampling and restart controls. Samples are opaque; a valid background
does not change them. Normalization uses at most two sixteen-pixel RGB rows,
never RGBA storage. Fixed/adaptive Huffman coding, quantization, stream limits
and cancellation retain the established codec policies. Caller input and 8 KiB
fixed workspace are admitted, with optional adaptive event storage separately
admitted. Quality numbers remain specific to this encoder.

Transforms return new owned buffers and preserve precision and source pixels.
Outstanding raw orientation is retained until `AutoOrient` or decode with
`ApplyOrientation=true` applies it and resets it to Normal. Coded and oriented
dimensions are separately exposed by `ImageInfo`. Identification validates
container structure and checksums, but does not validate all compressed pixels.

JPEG inspection validates the complete marker/scan-header, stuffing, EXIF and
EOI/EOF structure without decoding entropy symbols. Noncancelable inspection
may batch ordinary and stuffed bytes within an admitted 4 KiB raw span; logical
work, validation order and the cancelable scan's polling remain unchanged.

## Drawing and compositing

`ImageDrawing` mutates caller-owned canvases in coded coordinates. It does not
apply pending EXIF orientation or change dimensions, precision or orientation.
Destination rectangles clip to the canvas. Negative extents are invalid; zero or
fully clipped rectangles do nothing after argument/ownership/memory validation.
Source rectangles for `CopyRegion` must fit, and exact copying requires matching
depths. All four samples, including hidden RGB, are copied. `FillRectangle` replaces
all four channels rather than blending. `RgbaColor` uses 16-bit straight samples;
`From8` expands by 257 and eight-bit fills round `(sample + 128) / 257`.

`Composite` implements [source-over](https://www.w3.org/TR/compositing-1/#porterduffcompositingoperators_srcover)
in encoded sample space without ICC/linear-light conversion. Source and canvas may
independently use 8 or 16 bits. Coverage is `a = as + ad*(1-as)` and straight color
is `(Cs*as + Cd*ad*(1-as))/a`; integer rational arithmetic keeps coverage unrounded
until final channels, rounding positive halves upward. Zero source alpha leaves
every destination sample unchanged, including hidden RGB. Other samples round
independently, including when tiny nonzero coverage rounds to zero alpha.

Self-copy and self-compositing use the original source and destination samples,
traversing rows/chunks/pixels in a safe order without a snapshot raster. Distinct
source buffers remain unchanged. No buffer ownership transfers or full-frame
temporary copies occur. Working admission counts each distinct supplied buffer
once. Geometry limits apply to the clipped affected rectangle. Work charges four
units per copied/filled pixel and sixteen per composited pixel, before chunks of
at most 4096 or 256 pixels respectively. Cancellation/work exhaustion can leave
completed chunks in traversal order; argument, geometry and memory failures occur
before mutation. Callers synchronize mutable pixel access.

## Supplied TrueType fonts

`TrueTypeFont.Load` copies a standalone TrueType face into immutable owned storage.
It supports Unicode `cmap` formats 4/12 (preferring 12), short/long `loca` and
horizontal metrics with repeated advances. Glyph zero represents absent characters.
`GetGlyphIndex(Rune)` and `GetGlyphMetrics(Rune/int)` return unscaled font-unit
values; glyph bounds enclose control points, with upward-positive font Y. Empty
glyphs have zero bounds. Ascender/descender/line gap come from `hhea`; face style
comes from the supplied `head` flags. There is no synthetic styling or system-font
lookup. Returned metric values and the loaded face survive changes to caller input.

Loading checks table ranges/order, selected character mappings, glyph locations
and headers. It does not validate every outline, execute hint instructions
or verify checksums. Font collections, CFF/WOFF and unsupported mapping/table
profiles raise typed `FontContainer`, `FontMapping` or `FontTable` unsupported
features, with no image format assigned. Malformed structure raises `ImageDataException`.
The implementation follows public [OpenType table definitions](https://learn.microsoft.com/en-us/typography/opentype/spec/otff).
Layout decodes and validates the outlines it uses.

Input, working-memory, item and work limits plus cancellation apply. Load admits
supplied input plus one owned copy and a coarse 256-byte descriptor allowance
before allocation. `StorageBytes` reports retained bytes for caller reservations;
subsequent lookups admit that font storage and charge bounded search/metric work.
There is no mutable global font cache or per-lookup font copy.

`TextLayout.Create(font, text, TextOptions)` returns immutable logical
left-to-right layout in pixels. `FontSize` is pixels per em; style comes from the
selected supplied face. Supported ordinary horizontal legacy `kern` format-zero
tables are applied unless `Kerning` is false. There is no bidirectional reordering,
GSUB/GPOS shaping, synthetic style, variation-axis selection or hint execution.
Malformed UTF-16 is an argument error; missing scalars use glyph zero.

`WrapWidth` defaults to infinity. Greedy wrapping drops separator spaces/tabs at
automatic breaks, preserving explicit leading/trailing whitespace otherwise.
Overlong words break between Unicode scalars; an individual glyph may overflow.
Nonbreaking spaces are not separators. CRLF is one break; CR/LF and Unicode line/
paragraph separators also break lines, including empty lines. Tabs advance to the
next `TabSpaces` space-width stop. The first baseline is `Ascender * scale`;
subsequent baselines advance by `(Ascender - Descender + max(0, LineGap)) * scale
* LineSpacing`. Empty text has zero lines/bounds.

`GetGlyph`/`GetLine` return value copies, with original UTF-16 source offsets for
glyphs. `LayoutBounds` covers nominal line boxes and advances; `InkBounds` covers
actual quadratic extrema, excluding spaces. `Bounds` is their union, preserving
negative overhangs. `TextBounds.Translate` positions bounds, and `FitsInside`
checks overflow. No scale-to-fit search or raster allocation is implicit.

Outlines follow the public [glyf definition](https://learn.microsoft.com/en-us/typography/opentype/spec/glyf):
packed/repeated flags, implied quadratic points, component transforms and point
attachments, including phantom points. `USE_MY_METRICS` inherits component phantom
metrics independently of its outline transform. Instructions are skipped; offsets
are unhinted, with unscaled offsets when neither offset flag is present. Cycles
are malformed; nesting beyond 64 glyph nodes raises typed `FontOutline` unsupported.
Point/contour/component counts, coordinate reads and work are bounded.

Layout owns private placements, lines and the needed decoded geometry; it retains
the immutable face, not the source string. `StorageBytes` includes that face and
coarse retained capacities for caller reservations. Admission includes borrowed
UTF-16 input while preparing, temporary component/flag/kerning storage and old/new
capacities during growth. Extent dimension/pixel limits apply without a hypothetical
RGBA frame. Work/cancellation checks cover decoding, layout and curve bounds.

`ImageDrawing.DrawText(canvas, layout, x, y, color)` renders a prepared layout in
coded canvas coordinates. The position is a finite continuous pixel origin;
destinations clip to the canvas. The layout/font stay immutable and reusable;
canvas storage and surviving outputs remain caller-owned. No orientation, font
lookup, gamma adjustment or full-frame copy is implicit.

Rendering uses nonzero winding within each glyph, then source-over in layout
order. Quadratics subdivide until midpoint deviation from the chord is at most
1/1024 pixel per coordinate (maximum 24 subdivision levels). Linear trapezoids,
split at vertices/intersections, integrate pixel coverage in binary64; coverage
rounds to 0..65535. Sixteen-bit color and coverage multiply without intermediate
alpha/depth rounding; exact rational straight-alpha blending rounds positive
halves upward only at final canvas samples. Zero effective source alpha leaves
all destination samples unchanged, including hidden RGB.

Admission counts canvas/layout storage, prepared edges/events/crossings, one
coverage row, a coarse 4 KiB frame/descriptor allowance and 1600-byte subdivision
stack. Old/new capacities overlap during growth. Edge/event item limits, work,
clipped geometry and cancellation apply. All argument/geometry/workspace admission
precedes mutation; later work/cancellation may leave completed chunks (at most
128 pixels each) in glyph, row and left-to-right order. No rendered glyph cache
or whole-frame mask is retained. Different rasterizers' curve approximation and
fixed-point scaling can produce different antialiasing; native development image
checks are not pixel identity or warm performance claims.

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
RGBA8 nonmixed cached resizes may emit rows in a separate byte-depth
routine. Vertical tap order, sums, divisions, byte margins, clipping, packing
and original-order replay stay identical. Odd cache-row widths require no
padding or additional allocation; admission and cancellation remain unchanged.
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
reuse up to eight-column strips and at most ten rows/taps per axis on
AVX512F/AVX2/AVX/SSE4.1 hardware. Below vertical scale 2.25, nine row slots hold
an eight-column strip; larger scales retain six columns and ten slots. This
optional path admits 4 KiB before its fixed stack workspace (4,080 bytes of
arrays); it adds no heap workspace or persistent state. Checked byte offsets
address at most 25 decoded source pixels per reused row. Adjacent horizontal
accumulators share 512-bit lanes, retaining each column's original tap order
and separate multiply/add; unmatched taps stay in their original half. Paired
quotient guards retain the conservative `2^-40*16^2` absolute-error budget and
exact unsigned byte premultiplication.
Uncertain rounding, small normalization or tiny alpha returns to the complete
original-order operation after the stack frame and admission unwind. Speculative
strips leave work accounting untouched; success charges the complete original tap
cost. Every sample remains exact, with bounded cancellation. Other directions,
supports and smaller budgets retain the existing paths.
RGBA16 reduction may use a separate word specialization of the same six-column,
ten-row strip strategy, with the same 4-KiB admission and 3,640-byte fixed arrays. Full
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
products are at most 65025 and widen to binary64 without quantization. Paired
products may accumulate in independent alternating-tap streams, folded once per
row. At most 37 dependent additions per stream/fold remain within the original
72-term error bound. Checked relative offsets, the `2^-40*72^2` budget and
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
RGBA16 enlargement on one or both axes may use a separate fourteen-column/five-row strip
cache when the full row cache cannot be admitted. Its optional 4-KiB admission
covers 4,000 bytes of fixed arrays. At most 21 source pixels are decoded once per
reused row; exact unsigned word products widen to binary64 without quantization.
Each horizontal/vertical accumulation retains tap order. Paired and single word
outputs use the conservative `2^-40*16^2` budget and depth-scaled quotient guards.
When height is unchanged and the original binary64 center is exactly integral,
the cardinal vertical kernel has one unit tap; zero-weight source rows may be
omitted. Original coordinate evaluation, full support admission and work remain.
Uncertain ordinary or negative-alpha samples replay the original order locally;
tiny-alpha pixels use an impossible completed-pixel marker in the private output.
After the non-inlined strip frame returns and releases its admission, a timed
scan replays only marked pixels with original color arithmetic and exact rational
alpha scratch, charging its additional work. No marker reaches a returned image.
Complete work, bounded cancellation and independent owners remain. No heap
workspace or persistent state is added; existing byte and reduction kernels stay
unchanged. Other directions, hardware and smaller budgets retain prior paths.
RGBA8 enlargement on one or both axes has a separate byte specialization of the
fourteen-column/five-row cache, with the same 4-KiB admission and 4,000 bytes of
fixed arrays. Exact byte products, original tap order and byte-depth quotient
guards use the same `2^-40*16^2` budget. The same original-coordinate equality
check may omit zero-weight vertical rows when height is unchanged. Ordinary or
negative-alpha uncertainty replays locally. Tiny-alpha pixels are marked only
in the private unfinished
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
Up to eight output rows reuse six source-column slots, decoding and filtering
adjacent columns together. Checked source-row offsets fit bytes; 3,972 bytes of
fixed arrays fit the unchanged admitted 4-KiB workspace. Scales above 2.125 retain
four-row bands. Exact unsigned word products and unrounded
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
width-growth/height-reduction supports. Up to eight output rows reuse six
source-column slots, decoding and filtering adjacent columns together; scales
above 2.125 retain four-row bands. Checked source-row offsets fit bytes, and
3,972-byte fixed arrays fit the unchanged 4-KiB admission. It may also preserve width when the existing vertical row cache is
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
For output widths through 16,512, horizontal premultiplied sums fit signed
32-bit integers and may be factored before widening for the vertical weights.
Integer distributivity preserves every final sum; wider outputs retain the
original four-tap arithmetic. Workspace, work charges and rounding are unchanged.

RGBA16 bilinear enlargement may use the same four-output strategy with exact
64-bit horizontal intermediates, reusing at most four operation-local rows.
Its optional workspace is `128 * outputWidth + 64` bytes and leaves room for
exact tiny-alpha scratch; tighter budgets retain the existing paired-row path.
Unsigned full-word premultiplication and output-array admission keep integer
sums below 2^62. A conservative precision bound covers final binary64 conversion,
with original rounding near ambiguous boundaries. Intermediates are never rounded
to pixels or reused across operations, and admission is released on every exit.

RGBA16 bilinear reduction on both axes may use a separable six-column row
cache when each scale is at most 2.5 and the required SIMD is available.
Its 3,640 bytes of fixed arrays fit an optional 4-KiB reservation after existing
horizontal geometry. Exact unsigned premultiplication and unrounded binary64
intermediates use the established `2^-40*16^2` word error bounds; guarded output
pairs replay original tap order near rounding boundaries. Tiny alpha declines
the specialization, releases its frame and reservation, then uses the existing
exact-alpha path. A successful pass charges the complete original tap cost;
declined speculative work leaves that charge to the original path. Cancellation,
independent outputs and tighter-budget fallbacks remain. No heap workspace,
persistent state or output cache is added.
RGBA8 reduction may use a separate byte specialization at the same scales and
4-KiB admission. Exact signed-32-bit premultiplication precedes unrounded row
reuse. Normalizers at least one and alpha above the tiny-alpha boundary permit
conservative 1e-4 RGB / 1e-6 alpha rounding margins under the same error budget.
Uncertain ordinary-alpha pixels replay the original tap order; tiny alpha
unwinds before exact scratch. The word kernel and earlier enlargement paths
remain unchanged; no heap workspace or persistent cache is added.

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
zlib window and the exact filtered-raster byte count before raster decisions
are published.
For noninterlaced inputs with at most 4 MiB of filtered data,
the same walk may also materialize bytes in a private, exactly admitted buffer.
Selection first ensures that this buffer, framing scratch, the original inflater
allowance, result, rows and optional read buffer all fit. Complete code trees
and a valid Adler checksum are required before using those bytes; uncertain
trees or checksums retain the original BCL row reader without repeating framing
work. Cancelable framing retains both per-symbol cancellation checks and exact
prefix-bit charges. Inside each compressed block, the reader admits exact bit
charges against a saved remaining budget and publishes completed work on every
exit without a new cancellation poll. Count-only framing uses a separate loop
from materialized-output dispatch, retaining the same literal, length, distance,
truncation and atomic-charge checks. Completed work survives cancellation.
The private whole-stream checksum checks cancellation every 4096 bytes.
When full filtered storage cannot fit, noninterlaced RGBA8/16 may instead use a
separate streaming framing walker. A 64 KiB sliding buffer retains 32 KiB of raw
Deflate history; raw row payloads occupy only the private result, with at most
64 KiB of row-filter bytes. Predictors and byte conversion run after each
original row work charge. Taller inputs retain the original reader.
Selection preserves the original
result/two-row/256 KiB peak; framing scratch borrows 4 KiB of that allowance.
Only complete framing, complete trees, valid filters and the Adler checksum
permit row reconstruction and publication. Other outcomes
replay the original BCL raster reader without repeating framing work. The
original validator/materializer, bit charges and cancellation checks remain.
PNG encoding may fold chunk payloads of at least 64 bytes through four
independent polynomial residues on supported PCLMUL/SSSE3 hardware. The existing
normal PNG polynomial powers and generated bit-order permutations are reused;
the chunk-type CRC seeds the payload, residues combine in order, and the final
32 zero bits are appended once. Smaller payloads and other hardware retain the
original chunk path. Chunk bytes, scalar tails, write sequence, admission and
cancellation remain unchanged; no per-operation workspace is added. Decode and
metadata checksum entry points keep their original arithmetic and polling.
Byte grayscale/RGB/grayscale-alpha encoding may pack bounded RGBA row spans
after each original 4,096-pixel cancellation checkpoint. Supported AVX-512
hardware selects complete groups of 16 pixels; invalid groups retain scalar
RGB-before-opacity validation and the first failing pixel. Channel bytes,
compression, writes, work and admission remain unchanged, with no new workspace.
PNG Up reconstruction may add nonoverlapping byte rows with safe 512/256/128-bit
SIMD and scalar tails, independently modulo256 at either precision. Original
64 KiB cancellation boundaries remain; overlapping rows and unsupported hardware
retain scalar behavior. No workspace, admission or work charge is added.
Other tight layouts and Adam7 retain the original reader when full filtered
storage cannot fit.
The original bit/row work, error order, precision and pixel contracts are unchanged.
Filtered buffers may reuse two released exact-size arrays with a combined 4 MiB
idle ceiling, separate from pixel and compressed storage. Every delivered byte
is materialized before use, and leases end before requested orientation.
Concatenated IDAT bytes may reuse at most two released exact-size private arrays,
whose combined idle bytes stay within 4 MiB independently of pixel storage.
Every byte is copied before framing;
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

The default `Optimal` compression policy uses the BCL's smallest-size search
for 8-bit gray, RGB and gray-alpha output, avoiding the weaker compression of
the runtime's default search on these layouts. Other formats and explicitly
selected `NoCompression` or `SmallestSize` retain their backend selection;
RGBA `Fastest` follows the policy below. This is an implementation policy, not a promise of a
particular numeric backend level or exact compressed bytes. Samples, filter and
chunk bounds are unchanged.

RGBA `Fastest` uses the BCL's streaming fast adaptive compressor at numeric
backend level2, rather than numeric level1's fixed-tree quick compressor. The
policy applies identically to stock/caller streams, live cancelable operations
and admitted tight budgets. Other color types retain their existing selection.
The zlib compression-class flag is1 for this fast backend; numeric backend levels
and encoded bytes are implementation choices, not a stable compressed-byte
contract. Full samples, filter None, IDAT chunk bounds, grouped cancellation,
logical row work and base memory admission remain unchanged. The library adds no whole-payload
staging, independent entropy recoding, per-image Huffman tables or output cache
outside the BCL compressor. Historical level1 recoding output remains losslessly decodable.

RGBA16 encoding packs complete RGBA words into big-endian rows using safe span
conversion in groups of at most 4096 pixels. RGBA8 copies complete rows in the
same bounded pixel groups. Cancellation precedes each group; packing preserves
full samples/hidden RGB/alpha, logical row work and admitted row storage under
every compression policy. The selected compressor determines lossless encoded
bytes. Other color types retain their existing sample validation.

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
RGB16 Paeth reconstruction uses six independent byte lanes with safe Vector128
dispatch. Loads stay within both rows and SIMD stores change only six bytes.
Scalar tails, overlapping/short previous rows and other hardware retain the
original recurrence, tie order and modulo256 arithmetic. Cancellation is checked
within64KiB; work and memory admission are unchanged.
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
Palettes of up to sixteen entries can form channel vectors for SSSE3 byte
shuffles. A group with any invalid index resumes scalar traversal before its
first store, preserving the original valid prefix and cancellation/error order.
Absent/partial alpha and scalar tails remain exact; no padded palette storage is
introduced. Larger palettes retain their existing expansion paths.
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

Subsampled YCbCr interiors may reconstruct sixteen pixels with exact integer
sixteenth interpolation and a certified float color matrix. Chroma and luma are
exact float inputs. Coefficient differences contribute less than `2^-16` each;
at most four float operations contribute at most `2^-16` each, with intermediates
below512 in magnitude. Including binary64 reference error, the absolute bound
is less than `2^-13`. Clipping does not increase error. A block is accepted only
when every channel lies farther than this bound from a rounding half; otherwise
the original eight-pixel binary64 path writes the block. Opaque alpha, scalar
edges/tails, source/owner, per-row work/cancellation and memory admission remain.
No approximate decoded byte escapes the certificate or new workspace is added.


Full-resolution direct RGB planes pack their already rounded/clipped bytes into
opaque RGBA directly, with AVX2 batches and an exact scalar tail. Equal sampling
factors give zero interpolation fractions; no further rounding is needed. The
original per-row work/cancellation admission is retained, with no new workspace.
Subsampled direct RGB keeps the original interpolation and rounding.
For horizontal 2:1 RGB sampling with full vertical resolution, centered coordinates
are exact quarters. Rows use equivalent integer quarter weights, AVX2 batches
and a scalar edge/tail path, preserving the original binary64 result and opaque
alpha. Equal sampling-factor representations are supported. Existing per-row
work/cancellation checks remain; no extra workspace or pixel cache is introduced.
For direct RGB with horizontal and vertical 2:1 sampling, rows combine two
component rows with exact sixteenth weights and one final rounding. AVX2 batches
and scalar edges/tails retain the original binary64 pixels, clamped borders and
opaque alpha without quantizing intermediate rows or adding workspace. Existing
row work/cancellation checks and equivalent sampling factors remain unchanged.

Baseline entropy traversal retains exact logical work charges in a local block
budget, including cancelable operations. Completed charges are published on every
exit without another cancellation check. Noncancelable blocks may also keep runs
of ordinary short AC symbols in local reader state, preserving the existing atomic
fused prefix/amplitude charge and original fallback state.
Noncancelable runs may refill a local 64-bit cache with four ordinary bytes and
reuse its lookahead across symbols. Only bytes required by completed symbols
are committed; unused whole bytes are returned before every exit. Stuffing,
markers, long/invalid codes, short tails and tight work budgets use the original
fallback with exact logical bits, partial coefficients and work/error ordering.
With a live token, consecutive ordinary AC symbols may reuse a local 64-bit
cache, safely refilled with four ordinary bytes at a time.
Prefix and amplitude retain separate work admissions and cancellation checks.
Unused whole bytes are returned before publishing the original reader position
and pending bits. Stuffed bytes, markers, incomplete/long codes, syntax failures
and tight budgets retain the original traversal and partial-failure state. No
additional table or heap workspace is introduced.

Progressive scans retain coefficients in spectral order. After all scans and
component DC progression are validated, reconstruction reuses the already
admitted approximation array to map each block to natural order. On AVX2/BMI2,
correction traversal can discover one nonzero-position mask per refinement block,
reuse it across symbols and update it only when a coefficient is inserted. Zero
runs count only zero positions; grouped bits visit nonzero positions in spectral
order while preserving insertion boundaries and signs. Each correction retains its original
logical work charge. Cancellable tokens, incomplete entropy, tight work budgets
and unsupported hardware retain scalar correction reads and failure precedence.
With AVX2/BMI2, no active local-work scope and at least 2,048 remaining work
units, whole AC refinement blocks may retain cursor, bit cache and work locally;
each block costs less than that bound. Noncancelable correction fields retain
safe grouped reads. Live-token blocks preserve every original Huffman, sign/EOB
and per-correction-bit cancellation check and immediate coefficient update.
Live-token blocks may also refill four ordinary lookahead bytes at a time.
Unused whole bytes are returned before publishing the logical cursor and pending
bits. Stuffed bytes, markers and short tails publish prior state before using
the original reader; cancellation and format/overflow failures publish completed
work and partial coefficient changes at the original logical boundary.
Every exit publishes exact reader/work state without replacing the original
failure. Tighter budgets and unsupported hardware retain the preceding masked
or scalar traversal. No additional table or heap workspace is introduced.

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
bits. Other calls use image-independent canonical AC codes derived from a
geometric zero-run/amplitude-size model; DC codes remain four bits. The shared
immutable AC table is constructed once, covers all 162 supported symbols and
reserves the all-ones code, with a maximum length of 16. Its weights are
`2^max(0,20-run-size)`, with EOB weighted `2^18` and ZRL weighted `2^4`.
No image statistics or corpus-fitted constants select this table. This selection does
not change quantized coefficients or decoded samples; encoded bytes may differ
with stream, cancellation or resource options. Optional restart intervals count
MCUs and reset DC predictors.
The fixed policy deliberately supersedes the original eight-bit AC codes;
historical quality reports remain unchanged and current decoded samples must
match them exactly. Encoding retains the same source-plus-8192-byte admission
and logical work/callback/failure ordering. Prevalidated canonical codewords skip
repeated range validation; general amplitude entry points retain their checks.
On AVX2/BMI2 x64, fixed-block emission builds a nonzero occupancy mask with eight
row comparisons and bit deposits into the standard zigzag positions. It visits
only nonzero AC coefficients, retaining ZRL/EOB, amplitudes, ordered work charges,
callbacks and partial-failure state exactly. Eight row masks are generated from
the independent zigzag construction; they are shared format state, not image
storage. Other processors retain the scalar traversal. No workspace is added.
The fixed AC traversal may fuse bit emission, retaining bit state locally and
publishing it on every exit. Each code/amplitude still charges work and checks
cancellation at its original position; byte callbacks, stuffing, quotas and
partial-failure state remain exact. Amplitudes derived from the already-validated
category fit the original range check by construction. No heap workspace or
additional admission is needed.
Decoding a new table with nine/ten-bit codes uses the existing additional 2 KiB
prefix lookup; original short-table input admission is unchanged.
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
AC planning may reuse the fixed encoder's safe AVX2/BMI2 scan occupancy to visit
only nonzero coefficients. Events, histograms and range-error order stay exact;
other hardware retains scalar traversal, with no additional workspace/admission.
Lower output quotas retain adaptive coding and enforce every write; combined
symbol writes apply only when conservative output and work bounds are satisfied.
When the whole packet's conservative bound fits the output quota, a
noncancellable stock `MemoryStream` may emit adaptive entropy bytes directly
through the bounded writer. Byte stuffing, restart framing, work charges and
partial-write failures remain identical. Caller stream subclasses, cancellable
operations and tighter quotas retain the original per-write checks.

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
Adaptive RGBA8 4:2:0 encoding may share unpacking and alpha flattening between
the two chroma components on AVX-512/AVX2. An optional, charged 64-byte stack
block retains Cr while Cb is transformed; insufficient remaining admission
keeps the original sampling path. Both paths preserve the original arithmetic,
quantized coefficients, component order, work charges and encoded bytes.
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
and zero-bit single leaves are unchanged. With noncancelable tokens, three
complete eight-bit red/blue/alpha trees may consume one locally staged triplet,
charging the original 24 logical bit reads and reading only required bytes.
Insufficient work/input and live tokens retain the original channel reads.
The existing tables and object storage suffice; no allocation or admission is added.
Large green-code trees can use a
12-bit lookup borrowed from the freshly zeroed, unwritten tail of their private
pixel array. Decoding stops using that lookup before the write cursor reaches
it, including after a backward reference crosses the boundary; every borrowed
word is overwritten before output is published. Stable one-pass code grouping
funds bounded lookup construction within the original logical work charge.
No additional allocation, working-memory reservation or output owner is added.
Output is RGBA8; VP8L dimensions are at most 16384
per side.

VP8 key-frame decoding implements arithmetic partitions, segmentation, all ten
subblock and four large-block prediction modes, coefficient contexts, integer
DCT/Walsh inversion and normal/simple loop filtering. Fully bounded horizontal
lines use eight-pixel integer word vectors when available, preserving scalar
thresholds, signed clamps, strong/weak rounding and work charges exactly. On
SSE2, bounded vertical lines can transpose eight independent row neighborhoods
to the same word arithmetic and transpose back. Loads/stores stay within each
row, neighboring bytes and original edge order remain exact, and no allocation
or additional admission is needed. Other lines retain the scalar path.
Zero frame-level filter
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
and repeated-color/previous-row references, without transforms, a color cache or general match search.
Opaque output uses a zero-bit alpha255 leaf when it saves payload bytes; other
literal tables remain fixed. `Effort` accepts 0..6 (default 4): zero writes
only literals; other settings permit repeated-color or previous-row sequences up
to `64<<effort` pixels. References may span rows and overlap their destinations;
the encoder compares the resident normalized pixels without a row or frame copy.
`Quality` accepts 1..100 but is ignored in lossless mode. VP8L payload length is counted before writing, so nonseekable output
requires no encoded-frame staging buffer. Both passes charge work and require
caller synchronization of mutable input pixels. Unsupported profiles fail
explicitly, without external fallbacks.

Counting omits whole-byte alpha fields and adds their exact size, or the 29-byte
singleton-header difference, before output preflight. Both passes still check
work/cancellation per alpha field, including zero-bit leaves. Source plus the
existing 8 KiB workspace suffices; no histogram, extra source scan or frame copy
is introduced. Alpha-bearing sequences preserve all hidden RGB and alpha samples.

`Lossless=false` emits a VP8 key frame with 4:2:0 chroma, a mathematical forward
4x4 DCT and the format's quantizers. `Quality` (default 85) maps to quantizer index
`((100-quality)*127+49)/99` with integer division. Quality 100 is still lossy.
Quantized coefficients round halves away from zero; the forward DC basis preserves
exact constant-block half ties.
Intra mode search prepares immutable neighboring samples once per luma block
and emits complete 4x4 predictions. Candidate order, exact error scores, tie
breaks, final mode writes and work/cancellation checkpoints remain unchanged.
The bounded stack scratch adds no allocation or mandatory resource admission.
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
Source conversion uses bounded views of the existing pixel storage, without a
normalized-raster allocation. The first row work check precedes storage access;
all row charges, matrix evaluation order and alpha-weighted rounding are retained.

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
Arithmetic normalization packs at most seven emitted bits together only within
already admitted partition capacity; growth and constrained failures retain the
original bit sequence. Immutable leaf paths are derived once from the encoder's
format trees, preserving each ordered probability and bit/carry work charge.
Encoded bytes, prediction choices, coefficients and partial failures are unchanged.
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
PNG decoding retains its separate 256 KiB inflater allowance, including the
optional managed-inflation path. Its additional filtered buffer is admitted
exactly; insufficient room selects bounded RGBA streaming or the original
reader. Geometry limits
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

Supplied streams remain open. Nonseekable input uses bounded buffering. Identification
may reuse private fully written input storage and inspect its acquired prefix
without an extra exact-length copy. Retention is bounded to one array per
power-of-two bucket up to4 MiB (less than8 MiB total); live calls never share a
lease. Original bounded-growth admission, read/work/cancellation and failure
ordering remain unchanged; the source is read through EOF and never exposed as
a borrowed input view.

Synchronous `Decode(Stream)`, including `ImageBudget.Decode`, retains a private
input lease through decoding instead of making a final exact-length encoded copy.
Acquisition admits 8 KiB scratch and simultaneous old/new growth capacities;
the actual retained capacity stays charged during decoding and is released on
every exit. Capacity growth is capped by `MaxInputBytes`. Tight working budgets
therefore account for retained capacity rather than only the acquired prefix.
Pixels remain independently owned after input release, disposal or reuse.
Input is read through EOF without querying length/position or borrowing an exposed
caller array. Work charges acquired bytes in addition to the existing codec work.
Existing synchronous `Identify` and async input policies are unchanged.

`IdentifyAsync(Stream)` and `DecodeAsync(Stream)` use `ReadAsync(Memory<byte>,
CancellationToken)` through EOF with `ConfigureAwait(false)`, then synchronous
CPU parsing/decoding. They neither use `Task.Run` nor invoke synchronous reads,
seek, length or position. Caller streams remain open. Identification keeps the
complete existing structural/EXIF contract. Decode preserves the same samples,
opt-in orientation and independent output ownership. These methods buffer encoded
input; use the separate header inspector for constant-space metadata acquisition.

`InspectHeaders(Stream)` and `InspectHeadersAsync(Stream)`, also on `ImageBudget`,
scan PNG/JPEG/WebP containers through EOF from the current position, leaving input
open. They report the same dimension, precision, alpha and IFD0 orientation fields
for supported headers using a fixed 8,448-byte working allowance: 8 KiB read scratch
plus coarse header/parser state. No encoded-input, metadata-block or pixel-sized
array is retained. EXIF entries are read individually without following thumbnails,
other directories or external offsets. Opaque PNG/WebP payloads and metadata gaps
are seeked when possible, otherwise drained. JPEG entropy is scanned for marker,
stuffing and restart framing, including metadata following a scan.

Inspection checks container lengths/order, supported dimension/profile fields,
EXIF orientation and terminal EOF. It intentionally does not check PNG CRCs,
JPEG coding tables/entropy symbols or WebP compressed pixels. Existing `Identify`
keeps its fuller structural validation; `Decode` validates pixels. Configured
dimension/pixel/input/metadata/item/work limits still apply. No hypothetical decoded
RGBA array ceiling is imposed because inspection creates no raster. Large images
may require raised dimension/pixel limits. Read-ahead and seeked bytes count toward
input and work admission, along with container/EXIF items. Async inspection uses
actual `ReadAsync` and synchronous seek where available, never synchronous reads
or `Task.Run`. Shared reservations span awaits and settle on all exits.

The async reader admits an 8 KiB scratch array and actual old/new capacities
during growth. Capacity is capped by `MaxInputBytes`; the acquired prefix is
parsed directly without an exact-length final copy. The retained full capacity
stays charged throughout parsing/decoding and is released with its private lease.
One live operation owns each leased array; the same bounded idle input pool may
be reused only after release. Shared-budget async entry points reserve before
the first read and settle in `finally`, including across suspended I/O, failures
and cancellation. Surviving `BudgetedImage` pixels retain their own admission.
Limits, typed failures, cumulative consumed work and caller exceptions remain.
Callers must keep the input stream alive until the returned ValueTask completes.

`EncodePngAsync`, `EncodeJpegAsync` and `EncodeWebpAsync` run CPU encoding into
private admitted staging, then call `WriteAsync(ReadOnlyMemory<byte>, token)`
in blocks of at most 64 KiB with `ConfigureAwait(false)`. No worker thread,
concurrent codec, synchronous caller write/flush, seek or final `ToArray` copy is
used. Caller streams stay open. Keep image/supplied samples stable and output
alive until completion. The JPEG sample overload uses `ReadOnlyMemory<byte>`;
it preserves positive strides, source ownership and direct gray/RGB processing
without an RGBA frame. Corresponding `ImageBudget` methods retain reservations
until delivery finishes and settle consumed work on every exit.

Staging normally uses linked private chunks, each at most 64 KiB. Actual byte
capacity plus coarse 256-byte stream and 64-byte per-chunk bookkeeping allowances
count toward working memory. Chunk capacity also respects the remaining working
and output allowance. JPEG can use a fixed stock MemoryStream backed by a private
array, admitted alongside codec/event workspace, to retain the existing adaptive
Huffman policy. Capacity is bounded by the existing stuffed-entropy upper bound
and `MaxOutputBytes`; the original resource/cancellable fixed-coding fallback
remains available. No existing synchronous encoder path is changed.

Private output reuse retains at most two exact arrays totaling 4 MiB; active
operations never share a lease and only fully written prefixes are delivered.
Unused array tails are cleared before delivery, including when a caller recovers
the backing array from ReadOnlyMemory. Codec work, staged-copy bytes, cleared-tail
bytes and delivery-attempt bytes count toward `MaxWork`.
Errors during staging leave caller output untouched. Delivery errors/cancellation
may leave a written prefix; discard it when atomic publication is required.

Stream
encoding can leave partial output on failure: callers needing atomic publication
must stage output themselves. No filesystem or process capabilities are used by
production code.
