# Lokad.Image contract and implementation status

Target: a dependency-free safe C# library for .NET 10, providing still PNG/JPEG/WebP
codecs and basic image editing. This document distinguishes implemented behavior
from required future functionality.

The development JPEG encoder uses one managed reference-compatible baseline
implementation for grayscale/4:4:4/4:2:2/4:2:0. It accepts RGBA8/16 images and caller
Gray8/RGB8 samples through sync/async streams. Caller grayscale overrides Sampling.
MCU restarts use fixed format tables for color and optimized tables for grayscale.
Color quality maps to qscale `1 + (100 - Quality) * 30 / 99`; default85 maps to5.
Grayscale uses the owned mathematical quantizer formula and its pinned backend's
quality value, with69 frozen. Superseded experimental encoder choices and switches
are removed. [Codec parity](docs/codec-parity.md) states the exact tested scope and
approved grayscale reference exception.

The next codec qualification target is [byte compatibility](docs/codec-parity.md)
with pinned FFmpeg encoders, then exact decoder samples, before further CPU or
allocation tuning. Only explicitly identified encoder-attribution metadata may
differ. Current size/quality and bounded pixel comparisons do not establish this
target; the existing implemented behavior below remains the current contract.

| Capability | Current status | Required target |
| --- | --- | --- |
| PNG decode | Implemented; full authored profile matrix with exact analytical and FFmpeg RGBA | All legal static color/depth combinations, palette/tRNS, five filters and Adam7 |
| PNG encode | Implemented; all four managed compression policies match14 frozen complete FFmpeg packets | Noninterlaced gray/RGB/gray-alpha/RGBA, 8/16-bit |
| JPEG identify | Implemented; live and authored header checks | Bounded baseline/progressive 8-bit inspection and orientation |
| JPEG pixels | Implemented; authored analytical cases, frozen FFmpeg expectations and live comparisons | Baseline/progressive gray/RGB/YCbCr/CMYK/YCCK decode; baseline gray/YCbCr encode |
| WebP identify | Implemented; live and authored header checks | Bounded simple/extended still-container inspection and orientation |
| WebP lossless pixels | Implemented; exact frozen and live FFmpeg comparisons plus authored transform/entropy cases | VP8L decode/encode with transparency and hidden RGB |
| WebP lossy decode | Implemented; exact frozen YUV/alpha reconstruction and bounded RGB comparisons | VP8 key frames with raw/compressed ALPH |
| WebP lossy encode | Implemented; complete pinned reference-packet controls | VP8 quality/effort, exact alpha, direct async delivery and shared admission |
| Exact edits | Implemented; hand-computed permutations and live FFmpeg checks | Crop, flips, quarter turns, transpose, all eight EXIF orientations |
| Resize | Nearest/bilinear/bicubic implemented; Lanczos3 implemented in development, performance qualification pending | Exact dimensions, aspect-preserving fit and alpha-aware nearest/bilinear/bicubic/Lanczos3 at 8/16 bits |
| JPEG caller samples | Gray/RGB/CMYK decode and gray/RGB encode implemented in development | Caller-provided 8-bit samples without mandatory RGBA; explicit source color space/inversion, quality and sampling |
| JPEG compatibility extensions | Implemented in development | CMYK/YCCK and JPEG 4:1:1 (4×1) / 4:1:0 (4×2) sampling |
| Stream extensions | Async Identify/Decode/Encode and fixed-workspace InspectHeaders implemented in development | Async I/O and bounded-memory large-image dimension/orientation inspection; avoid redundant encoded-input and full-frame copies |
| Drawing/compositing | Implemented in development | Alpha compositing, rectangle fills and rectangular pixel copies at 8/16 bits |
| Text | Supplied TrueType, logical/OpenType layout, Latin/Hebrew/Arabic shaping, alignment/fitting and 8/16-bit rendering in development; full replacement/performance qualification pending | Supplied faces, alignment/style, wrapping/line spacing, reliable bounds/fitting, combining marks and bidirectional/shaped text including Arabic |
| Failure details | Implemented in development | Typed exhausted-limit and unsupported-feature categories with recognized codec when known |
| Cooperative memory | Implemented in development; qualification ongoing | Borrowed host-independent shared-root hooks, pre-allocation capacity/peak admission and explicit retained-image/font/layout lifetime accounting across public operations |
| Compression efficiency | High-priority ongoing work | Lossless WebP size competitive with the fixed FFmpeg/libwebp reference; JPEG size and reconstruction quality assessed together, with matched quality and no material grayscale regression; content/sampling scope stated |
| Source metadata | Typed encoded representation | `ImageInfo.SourceColor`, `SourceAlpha`, `SourceComponentCount` and source `BitDepth`; consistent sync/async Identify and bounded InspectHeaders |

GIF, APNG and animated WebP are rejected. Arithmetic/lossless/12-bit JPEG,
general progressive JPEG encoding, indexed/Adam7 PNG encoding and metadata round-tripping
are deferred. ICC conversion remains opt-in and is not currently implemented;
EXIF reorientation is opt-in. Planned extensions preserve resource limits,
cancellation, pixel precision, caller ownership and independent output lifetimes.

## Pixels and ownership

Development targets package version 0.2.0; published 0.1.0 remains unchanged.
Replacement qualification is pending cooperative-memory, text-layout/shaping,
Lanczos3, packaged-consumer and warm-performance gates. Default option values
do not establish compatibility requirements.

### Cooperative memory admission

Set `ImageLimits.MemoryBudget` to a caller-owned `IMemoryBudget` adapter and
forward the same root through each operation's limits/options. The interfaces
`IMemoryBudget`, `IMemoryScope` and `IMemoryReservation` introduce no governor
implementation or dependency. Scopes share the root, start empty and refund
their subtree on idempotent nonthrowing disposal. Admission is immediate and
thread-safe; zero is valid, negative estimates throw, refusal changes nothing,
and disposed owners reject admission/resizing. A reservation reports zero after
its disposal or an ancestor's disposal. Callers serialize disposal with active
users; disposing a scope does not cancel work or dispose runtime objects.

Substantial owned capacity is reserved before allocation, including overlapping
old/new storage during growth. Borrowed encoded input, sample buffers, images,
fonts, layouts and output streams are not charged again; actual copies are.
Local `ImageLimits` still count supplied inputs/results as before. Temporary
execution owners refund on success/failure; images, fonts and layouts retain
independent admission until their explicit `Dispose`. Prepared layout outlines
remain usable after their source font is disposed. Stop all users before disposing
these objects or a caller-owned ancestor. No refund relies on garbage collection.

Governed operations select uncached pixel/coefficient/input/output storage before
admission and never populate uncharged process-wide pools. Minor objects and
fixed runtime storage are approximate; admission does not measure heap/RSS and
callers must retain runtime/collection headroom. Asynchronous reservations remain
live until actual reads/writes and cleanup complete, including after cancellation.
Refusal throws `ImageLimitException` with `WorkingBytes`/`SharedBudget`, ends the
operation and refunds relinquished storage. It never triggers a retry, wait,
eviction or lower-memory fallback. Cancellation and actual allocation failure
remain distinct. Local input/output, geometry, container and memory limits remain independent.
There is no cumulative CPU allowance; cancellation is cooperative.

### Pixel buffers

`ImageBuffer` owns a tightly packed top-to-bottom RGBA byte array. Channels use
straight alpha. `GetRow8` exposes byte samples; `GetRow16` exposes native-endian
unsigned 16-bit samples. 8-bit images use four bytes/pixel, 16-bit images eight.
Rows have no padding. Dispose drops ownership; callers must cease using
previously returned spans, which safe C# cannot revoke. Released PNG decode,
crop and rotation/flip storage may be reused by a later result. Complete-overwrite
operations reconstruct every output sample before publishing their owner and
dispose private results on failure. Live buffers never share their pixel
storage. New `ImageBuffer.Create` buffers are zero initialized.
Without cooperative admission, released pixel storage may retain at most two exact-size arrays, with their
combined idle bytes capped at 4 MiB. Only released leases are eligible; active
owners never share storage. An unmatched size discards idle entries. Retention
does not change input/result byte admission, and oversized arrays are not cached.

JPEG decoding may also retain three idle exact-size coefficient arrays; owned
full-resolution CMYK can grow this to four. The combined cap remains 4 MiB.
Every lease is fully zeroed before parsing and returned after
its last use or a decode failure. Live decoders never share a lease. Original
coefficient working-memory admission remains in force.

`ImageCodec.DecodeJpeg` writes caller-owned interleaved 8-bit grayscale, RGB or CMYK
samples without an RGBA frame or encoded-input copy. Stride zero means packed;
positive stride must cover a row. Padding/suffix bytes are untouched. The supplied
input and complete destination spans count toward working admission and must not
overlap. Caller storage remains borrowed; codec scratch releases on return.
Failures may leave a completed pixel prefix; no buffer ownership is transferred.
`JpegDecodeOptions.SourceColorSpace` selects encoded Gray/RGB/YCbCr/CMYK/YCCK interpretation;
Auto preserves component-count, Adobe and RGB-ID inference. RGB output uses the
encoded-space color matrix at the documented integer precision. Grayscale takes decoded Y for
YCbCr and repeats gray unchanged. For encoded RGB, components are first rounded to
normalized8-bit RGB, then rational BT.601 Q15 studio luma and endpoint-aligned Q14
full-range conversion produce GRAY8. This is the same luma precision used after
CMYK multiplication; no RGBA intermediate or new owned scratch is required.
EXIF reorientation is opt-in and writes directly to the oriented destination
layout; returned dimensions/orientation describe those samples. ICC is not applied.

Four-component Auto interprets Adobe APP14 transform 2 as YCCK and transform 0
or an absent marker as CMYK. Transform 1 is unsupported for four components.
Canonical CMYK samples use 0 for no ink and 255 for full ink. YCCK applies the
owned inverse YCbCr matrix to `(255-C,255-M,255-Y)` while keeping K independent.
`CmykInversion.Auto` then inverts all four values for Adobe transform 0/2, leaving
untagged data unchanged; `None` and `Invert` override that convention explicitly.
These defaults accommodate the [documented inversion ambiguity](https://raw.githubusercontent.com/libjpeg-turbo/libjpeg-turbo/main/doc/libjpeg.txt).
RGB uses a device-color multiplication with a Q16 reciprocal derived as
`floor(65536/255)`: signed products shift16 bits without an added rounding bias.
For CMYK, inversion selects the color and black factors before multiplication.
For YCCK, half-resolution components expand from integer half-grid origins with
horizontal byte rounding before vertical rounding. Black/inversion is applied in
Y/U/V before the shared Q13 RGB matrix. Grayscale returns the adjusted Y for
YCCK; CMYK uses rational BT.601 Q15 studio luma on the multiplied RGB bytes, then
endpoint-aligned Q14 full-range conversion and final byte rounding. Canonical
CMYK output retains the stated ink interpretation and its own matrix precision.
Owned RGBA and caller RGB/grayscale share these output boundaries, including
opt-in orientation. No full-frame intermediate is required; YCCK RGB uses three
stack bytes. Ownership and memory-admission boundaries remain.
Original constant, varying, progressive, saturated and AC controls have exact
native pixel gates; the tested scope is in the decoder evidence ledger.
This is a device-color approximation with no ICC processing. JPEG K is never
alpha. CMYK output requires four encoded components; CMYK encoding is unsupported.
Four-component frames use the existing sampling-factor and ten-block MCU bounds.

`ImageCodec.EncodeJpeg` also accepts borrowed gray/RGB
samples and positive or packed strides. Input is neither changed nor retained.
Gray input always produces one component; RGB input uses `JpegEncodeOptions`
quality, sampling and restart controls. Samples are opaque; a valid background
does not change them. Normalization uses bounded RGB rows, never an RGBA frame.
Bounded rows feed the same reference-compatible component conversion, integer
transform, tables and entropy writer as image inputs. Borrowed source storage,
8KiB fixed workspace, table capacity and temporary tree-building peaks remain
separately admitted. Refusal stops the operation without another encoder path.
The fixed workspace includes a bounded MCU tile reused through signed chroma
filtering, normalization/filter plans and exact integer forward transforms and
quantization with SSE2/AVX2 acceleration and scalar fallbacks. RGBA16 subsampling
normalizes each bounded source row once, preserving single-round flattening
without additional storage. Quality
numbers remain specific to this encoder.

Adaptive4:4:4/4:2:2/4:2:0 and nonrestart grayscale encoding may retain packed entropy
records to avoid repeating conversion, transforms and coefficient traversal.
Initial data capacity is at most4MiB plus128bytes for ownership; one growth may
double data capacity to8MiB.
Growth admits old and new capacity together before copying, then releases the
old charge. Selection uses checked geometry and the declared local allowance,
including table-building workspace. For4:4:4, selection covers the worst single
growth overlap and future tables beside maximum record capacity. Tight local allowances select recomputation
before admission. Any admission refusal is terminal. Governed storage is private
to the operation and releases promptly. Calls without a memory root may reuse
the existing pool, whose retained ceiling remains4MiB. Writer-call
order and cooperative cancellation are preserved. Fixed-table color and restart grayscale paths do not use this owner.

Transforms return new owned buffers and preserve precision and source pixels.
Outstanding raw orientation is retained until `AutoOrient` or decode with
`ApplyOrientation=true` applies it and resets it to Normal. Coded and oriented
dimensions are separately exposed by `ImageInfo`. Identification validates
container structure and checksums, but does not validate all compressed pixels.

JPEG inspection validates the complete marker/scan-header, stuffing, EXIF and
EOI/EOF structure without decoding entropy symbols. Noncancelable inspection
may batch ordinary and stuffed bytes within an admitted 4 KiB raw span;
validation order and the cancelable scan's polling remain unchanged.

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
once. Geometry limits apply to the clipped affected rectangle. Cancellation
checkpoints occur before chunks of at most 4096 copied/filled pixels or 256
composited pixels. Cancellation can leave
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

Input, working-memory and item limits plus cancellation apply. Load admits
supplied input plus one owned copy and a coarse 256-byte descriptor allowance
before allocation. `StorageBytes` reports retained bytes for caller reservations;
subsequent lookups count that font storage toward local memory admission.
There is no mutable global font cache or per-lookup font copy.

`TextLayout.Create(font, text, TextOptions)` returns immutable layout in pixels.
`FontSize` is pixels per em; style comes from the selected supplied face.
The default `Shaping = TextShaping.Logical` uses logical left-to-right layout
with ordinary horizontal `kern` format-zero tables unless `Kerning` is
false. `Shaping = TextShaping.OpenType` enables supported Latin, Hebrew and Arabic
shaping and Unicode 17 bidirectional layout. `Direction` selects automatic,
left-to-right or right-to-left paragraph direction; the latter requires OpenType
mode. No synthetic style, variation-axis selection or hint execution is provided.
Malformed UTF-16 is an argument error; missing scalars use glyph zero.

OpenType mode uses the supplied face's default language system, with composition,
localized forms, Arabic joining forms, required/contextual/standard ligatures and
mark positioning. GSUB single/ligature/chained-context/extensions, coverage-based
contextual substitution (type 5 format 3), and GPOS
single/pair/mark-base/mark-ligature/mark-mark/extensions are implemented. Selected
cursive/contextual positioning and other lookup families raise typed `FontTable`
unsupported failures. GPOS value records and format-three anchors may reference
GDEF item variation data whose total correction is proven zero at the default
instance. Referenced indices, region coordinates and delta-row bounds are checked;
nonzero default corrections, device corrections and long-word positioning deltas
remain explicitly unsupported. This provides no variation-axis selection.
Other scripts and missing
required Arabic joining features are explicitly unsupported. Mirroring uses encoded
partners or the font's supported `rtlm` substitutions; a mirrored glyph without
either fails explicitly. There is no implicit font substitution or raster fallback.

`WrapWidth` defaults to infinity. Greedy wrapping drops separator spaces/tabs at
automatic breaks, preserving explicit leading/trailing whitespace otherwise.
Overlong words in logical mode break between Unicode scalars. OpenType wrapping preserves
grapheme/ligature clusters and reapplies positioning at final line boundaries;
an individual unbreakable cluster may overflow.
Nonbreaking spaces are not separators. CRLF is one break; CR/LF and Unicode line/
paragraph separators also break lines, including empty lines. Tabs advance to the
next `TabSpaces` space-width stop. The first baseline is `Ascender * scale`;
subsequent baselines advance by `(Ascender - Descender + max(0, LineGap)) * scale
* LineSpacing`. Empty text has zero lines/bounds.

`Alignment` selects start, center or end within finite `WrapWidth`, or within the
widest line when wrapping is unbounded. Start/end follow each OpenType paragraph's
direction. `GetLine().Direction` reports that direction. Measurement and rendering
consume the same retained placements and outlines.

`GetGlyph`/`GetLine` return value copies, with original UTF-16 source offsets for
glyphs. `LayoutBounds` covers nominal line boxes and advances; `InkBounds` covers
actual quadratic extrema, excluding spaces. `Bounds` is their union, preserving
negative overhangs. `TextBounds.Translate` positions bounds, and `FitsInside`
checks overflow. No fit search or raster allocation is implicit in `Create`.

`TextLayout.TryFit(font, text, width, height, out layout, options,
minimumFontSize: 1, fontSizeStep: .25)` searches a finite descending size grid
starting at `options.FontSize` and including the exact minimum. It returns the
largest fitting candidate, or false/null when no completed candidate fits.
Wrapping is capped at the target width. Fit uses the union `Bounds` extent,
including ink overhang: position the returned layout at
`(-layout.Bounds.X, -layout.Bounds.Y)` within the target area. Sizes need not
have a monotonic fit predicate across wrapping discontinuities.

The potential candidate count is bounded by `MaxContainerItems`. One retained
owner/admission context precedes allocation and survives a successful result.
Rejected geometry/capacities release before the next trial.
No image canvas or intermediate retained layout is created. Limits, admission
refusal, cancellation, unsupported features and allocation failures terminate
the call; they never trigger a lower-size retry or become a false/no-fit result.

Outlines follow the public [glyf definition](https://learn.microsoft.com/en-us/typography/opentype/spec/glyf):
packed/repeated flags, implied quadratic points, component transforms and point
attachments, including phantom points. `USE_MY_METRICS` inherits component phantom
metrics independently of its outline transform. Instructions are skipped; offsets
are unhinted, with unscaled offsets when neither offset flag is present. Cycles
are malformed; nesting beyond 64 glyph nodes raises typed `FontOutline` unsupported.
Point/contour/component counts and coordinate reads are bounded by input and item limits.

Layout owns private placements, lines and the needed decoded geometry; it retains
the immutable face, not the source string. `StorageBytes` includes that face and
coarse retained capacities for caller reservations. Admission includes borrowed
UTF-16 input while preparing, temporary component/flag/kerning storage and old/new
capacities during growth. Extent dimension/pixel limits apply without a hypothetical
RGBA frame. Cancellation checkpoints cover decoding, layout and curve bounds.

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
stack. Old/new capacities overlap during growth. Edge/event item limits,
clipped geometry and cancellation apply. All argument/geometry/workspace admission
precedes mutation; later cancellation may leave completed chunks (at most
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

`Lanczos3` uses `sinc(x) * sinc(x/3)` within radius three, with `sinc(x) =
sin(pi*x)/(pi*x)` and its continuous value at zero. It uses the same centers,
reduction widening and clamped edges. Per-axis weights are coalesced onto source
pixels and normalized in increasing source-index order. Cumulative normalized
weights round to multiples of `2^-30`, with an exact final sum of one. Signed
64-bit horizontal and 128-bit vertical accumulation retain all intermediate
precision; final alpha divides by `2^60`, and colors divide by the unrounded
alpha sum. Round halves away from zero, then saturate. These rules define pixels
independently of SIMD availability. A bounded cache holds at most 64 horizontal
rows, with separately admitted axis tables and one output-row accumulator.
Both depths preserve hidden RGB on identity; other zero-rounded-alpha output
has zero RGB. Limits/refusal/cancellation dispose temporary and unpublished
result owners, while successful results retain independent shared-root charges.
Final quotient calculation may use a projected estimate certified by a bounded
rounding interval or its exact integer remainder, with exact division otherwise. This changes neither rounding
nor resource/cancellation behavior and introduces no approximate pixels.
Optimized kernels must preserve these pixels, including exact-half and tiny-alpha
cases. Uncertain rounding uses the exact computation; unsupported hardware retains
portable fallbacks. Workspace is reserved before allocation, and shared refusal
terminates rather than selecting a fallback. The
[Lanczos3 certificate](docs/lanczos3-fma-certificate.md) records its rounding proof.

Filtering operates in encoded sample space with premultiplied-alpha accumulation,
followed by unpremultiplication. Round halves away from zero and clamp to sample
range. Filtered pixels whose rounded alpha is zero have RGB zero; exact permutations
and identity resize preserve hidden RGB. No linear-light or ICC conversion is applied.
Bilinear/bicubic retain their defined tap order and exact tiny-alpha handling.
Geometry and row caches are operation-local, admitted with source/result capacity
and scratch, and never make a refused shared admission recoverable.

All eight EXIF orientations dispatch to exact pixel permutations. Exact edits
preserve every sample and return independent storage. Orientation is retained until
explicit AutoOrient or opted-in decode applies it, then resets to Normal.
Cancellation is checked in bounded row/tile/sample intervals; failed private
results are disposed before publication. The
[cleanup review](docs/cleanup-qualification.md) explains retained bicubic kernels.

## Formats and metadata

`ImageInfo.SourceColor`, `SourceAlpha` and `SourceComponentCount` describe encoded
source representation, independently of decoded RGBA. `BitDepth` remains encoded
sample precision, or PNG index precision; decoded packed/indexed PNG becomes RGBA8.
PNG reports grayscale/RGB/indexed, explicit alpha channels, palette `tRNS`, color-key
`tRNS` or no alpha. Palette entries and transparency tables do not change index depth.
Component count includes explicit alpha; palette indices count as one.

JPEG reports one-component grayscale and models established by JFIF, Adobe transforms
or conventional RGB/CMYK component names. Unlabelled three/four-component models,
unknown Adobe transforms or conflicting Adobe declarations report `Unknown`, retaining
the known component count. This does not apply ICC, CMYK inversion, caller color
overrides or EXIF reorientation, or validate coding-table/entropy semantics.
WebP VP8 reports YCbCr and separate `ALPH` when present. VP8L reports RGB with four
coded components and `Channel` alpha even when its nonbinding alpha hint is unset.
`HasAlpha` keeps its existing header semantics: PNG channel/`tRNS` presence, or WebP
feature flags/alpha hint. It does not establish that a decoded pixel is nonopaque.

All sync/async Identify and InspectHeaders paths report the same source metadata for
valid input without decoding pixels. Bounded header workspace, validation boundaries,
limits, typed failures, cancellation, shared admission and borrowed streams remain.
The original six-argument `ImageInfo` construction/deconstruction is retained; manual
construction leaves descriptors `Unknown` and count zero until initialized. Record
equality and serialization include the new fields.

### PNG

PNG follows the [W3C specification](https://www.w3.org/TR/png/). Decoding supports
all legal static color/depth combinations, packed/indexed samples, palette/color-key
tRNS, five filters and Adam7. Validate signature, chunk lengths/names/order/CRCs,
geometry, transparency and the exact filtered-raster byte count. Unknown critical
chunks fail; unused ancillary payloads are skipped without decompression. Recognized
EXIF must precede IDAT. APNG is unsupported.

An owned bounded Deflate framing walk validates final blocks, stored lengths,
fixed/dynamic trees, repeats, distances and the declared zlib window. Validate
the zlib header and Adler checksum at the actual stream boundary; ignore unused
trailing IDAT bytes as permitted by PNG section11.2.3. Eligible noninterlaced
inputs may materialize filtered data within4MiB, or stream RGBA reconstruction
within the existing inflater allowance. Selection considers complete local
workspace before admission; a shared-root refusal remains terminal. Uncertain
format/checksum cases retain the validating reader, never publish speculative pixels.
Scratch and compressed buffers release before requested orientation.

PNG encoding is noninterlaced grayscale/RGB/grayscale-alpha/RGBA at8/16bits.
Gray output requires equal RGB; outputs without alpha require opaque samples.
There is no implicit grayscale conversion or alpha removal. Source metadata is
stripped. Filter None, complete-row stored blocks, IDAT payloads at most4096bytes
and neutral pHYs (x0/y1/unit0) follow the pinned reference policy.
Managed Deflate policies are stored level0 (NoCompression), greedy level1
(Fastest), lazy level6 (Optimal/default) and longer lazy level9 (SmallestSize).
Bounded search effort defines compression policy, not a caller fuel allowance.
History/dictionary/token storage is bounded, without full-frame filtered/token
storage. Fourteen frozen513×259 controls match complete reference packets:
four color types at both depths for Optimal, RGBA8/16 for the other policies.

Packed-gray tRNS qualification corrects the registered native decoder's omitted
alpha with the approved timed color-key transform. All72 authored profiles require
exact analytical/native RGBA;44 live controls check every key at both interlace
settings. This reference correction does not change managed pixel requirements.

### JPEG

JPEG follows [T.81](https://www.itu.int/rec/T-REC-T.81/en). Decode8-bit Huffman
baseline/progressive grayscale, RGB, YCbCr, CMYK and YCCK. Supported layouts include
4:4:4/4:2:2/4:2:0 and three-component4:1:1 (4×1)/4:1:0 (4×2), including equivalent
sampling representations within the component/ten-block MCU bounds.
Arithmetic/lossless/12-bit JPEG and progressive encoding remain unsupported.
All inverse blocks use the independently generated fixed-point cosine kernel;
[inverse rounding](docs/jpeg-inverse-rounding.md) defines dequantization, signed
row saturation, column precision and clipping. Thirty frozen gray/color controls
require exact native samples, including progressive refinement, EOB/split scans,
odd edges and restarts. Named CMYK/YCCK/411/410 controls also require exact native
samples. Older bounded comparisons are historical, not substitutes for these gates.

Caller grayscale/RGB/CMYK buffers, explicit color transforms and inversion,
orientation and padding contracts are specified under Pixels and ownership.
No mandatory RGBA or full-frame RGB intermediate is used. Coefficient planes,
Huffman tables, scans and transforms count toward local/shared memory admission;
replacement peaks are covered before allocation. Component coefficients release
after sample conversion. Every Huffman redefinition is charged, including replaced
tables. Cancellation checkpoints preserve valid reader/coefficient state without
cumulative work counters or partial-charge semantics.

Encoding emits baseline Huffman gray/4:4:4/4:2:2/4:2:0 (default420). Quality1..100
defaults85. Color maps to qscale1+(100-Quality)*30/99, with quantization derived from
normative H.262§6.3.11 data. Gray uses basis1+2(u+v)+uv and integer scale
Quality<50?5000/Quality:200-2*Quality, then clamp((basis*scale+50)/100,1,255).
Quality100 is lossy; equal numeric qualities across encoders are not equivalent.

Reference-compatible full-range BT.601 conversion, transforms, quantization,
Huffman construction and entropy are generated from source samples/options.
Color uses16-row MCU padding; gray uses8×8 blocks. Adaptive tables collect
frequencies before emission; eligible paths retain bounded entropy records,
otherwise recompute. Color restarts use mathematically generated T.81 AnnexK
tables; grayscale optimizes with predictor resets. No full-frame YUV copy is used.
One bounded18×18 tile and512-byte MCU share the8KiB fixed owner, alongside
separate coding-table/building owners. Admission refusal is terminal.

Images require opaque samples or explicit0xRRGGBB BackgroundRgb. Flattening in
encoded sample space and16→8 normalization use one nearest-integer rounding.
Caller gray/RGB is opaque and borrowed. The sole allowed packet difference is
the identified encoder-attribution COM in [codec parity](docs/codec-parity.md).
No source metadata is copied. Frozen packet claims name content/settings; they
do not imply arbitrary-content compression or CPU parity. Original quality/size
experiments remain historical producer evidence only.

### WebP

WebP follows [RFC9649](https://www.rfc-editor.org/rfc/rfc9649.html) and VP8 intra
sections of [RFC6386](https://www.rfc-editor.org/rfc/rfc6386.html). Simple/extended
still containers accept EXIF/XMP before or after pixel chunks. Animation is rejected.
Reserved/future VP8X fields are ignored as required by the format; short headers
and impossible canvas products fail. VP8 coded sides are at most16383pixels.
Video display-scaling hints do not resize the canvas; reserved color spaces fail.

VP8L decoding covers all four transforms/fourteen predictor modes, packed palettes,
spatial prefix groups, canonical codes, caches and overlapping references. Alpha
and hidden RGB stay exact even if the header alpha hint is unset. Trees/maps,
conversion overlap and rasters are admitted before allocation; temporary codec
owners release before optional orientation. Input/truncation checks and bounded
cancellation remain independent of memory admission.

VP8 decoding covers token partitions, probability updates, intra prediction,
integer inverse DCT/Walsh and normal/simple filtering. Empty partitions and
inconsistent lengths fail. Separate ALPH accepts raw/headerless VP8L with all four
inverse filters and modulo256 reconstruction. Reserved bits and informative
level reduction do not change samples. Raw copies check cancellation every64KiB.
Parser/mode owners release before alpha; YUV/alpha survive RGBA conversion.

Reconstruction matches frozen Y/U/V and alpha exactly. RGBA8 uses centered
bilinear chroma with edge extension and8-bit chroma rounding before limited-range
BT.601. The explicit Q13/Q15 studio round-trip, saturated16-bit intermediate and
15-bit truncation match164 frozen native/authored RGBA controls. The reference
normalizes through RGBA16 because its registered direct-byte conversion wraps
blue for one analytical saturated input; this approved reference normalization
does not relax managed exactness. No full intermediate RGBA frame is allocated.

EncodeWebp defaults to lossless. RGBA16 normalizes once by(sample+128)/257;
RGBA8, alpha and hidden RGB stay exact in lossless mode. Quality1..100 is ignored
for lossless; Effort0..6/default4 controls transform/reference search. Byte controls
use pinned libwebp1.6.0 quality75, matching method, exact=1, one CPU. Independent
prediction/subtract-green/palette/color transforms, matching/cache selection,
histogram clustering and canonical codes produce19 selected plus two forced
packet identities. Photograph/diagram are160032/7234B. Candidate choice uses coded
length/tie order, never available memory. Refusal/cancellation ends the call.

Prepared tokens/maps/codes own their capacities without borrowing source pixels.
Histogram/replacement overlap is admitted before allocation. Normalized working
pixels and losing candidates release before delivery; complete RIFF lengths and
padding are checked before writes. No full encoded-frame buffer is required.

Lossless=false emits VP8 key frames with4:2:0, Quality1..100/default85, Effort0..6.
Quality100 still quantizes. Pinned prediction, rate/distortion, trellis, source
clustering, quantization, probability, retry and filtering policies define bytes.
Use one token partition and sharpness0. RGBA16 normalizes once; luma and gamma-aware
alpha-weighted chroma match the undithered reference. Edges repeat source borders;
transparent cleanup follows lossy policy. Alpha is exact after normalization,
raw or headerless VP8L as chosen by that policy. ICC/reorientation is never implicit.

Twenty complete513×259 packet identities across22 source/settings rows cover
procedural/photograph/diagram, all efforts, named qualities and8/16normalization.
A separate5632² opaque Q75/method4 control matches its16787132-byte retry packet.
Photograph/diagram Q75/method4 are17688/7100B. There is no WebP metadata exemption.
Inputs and codec settings stay frozen; no reference packet/table is a build input.
Broader claims need their own declared scope, not a settings grid.

Source/analysis/frame owners release after preparation; alpha/partition owners
remain charged through actual sync/async writes. Lossy async writes partitions
directly. No shared refusal triggers waiting, retry or lower-memory encoding.
[Codec parity](docs/codec-parity.md) retains producer/packet details; CPU and
replacement qualification remain separate and open.

## Limits, failures and streams

Default positive immutable ImageLimits:32768pixels/side,32million pixels,64MiB
input/output each,512MiB working storage,4MiB metadata,100000container/EXIF items.
Codec/managed array-length ceilings also apply. Local memory includes borrowed
inputs/results as well as owned temporary capacity; shared admission charges only
owned storage and copies. Neither is a process heap/RSS limit. Geometry applies
to codec input/new results; an existing larger transform source still counts
toward local memory. No fuel, cumulative CPU allowance or deterministic partial
charge is provided. Use cooperative CancellationToken checkpoints for interruption.

ImageDataException means malformed data; UnsupportedImageException exposes Feature
and recognized Format; ImageLimitException exposes Limit and Operation/SharedBudget
Scope. Shared refusal reports WorkingBytes. Cancellation preserves the supplied
token in OperationCanceledException; actual allocation failure stays distinct.
Invalid arguments use standard argument exceptions. No message parsing is required.

Caller streams remain open. Identify/Decode(Stream) read through EOF into bounded
private capacity without a final exact-length copy or borrowing an exposed caller
array. Admit8KiB scratch and old/new growth overlap; retained capacity, capped by
MaxInputBytes, survives parsing/decoding. Returned pixels are independent.
Without a shared root, idle input reuse is bounded below8MiB; governed calls bypass
uncharged pools. Mutable inputs require caller synchronization.

IdentifyAsync/DecodeAsync use actual ReadAsync(Memory,token), ConfigureAwait(false)
and synchronous CPU parsing, without Task.Run or synchronous reads/length/position.
Storage survives all actual asynchronous users and releases on every exit.
Returned images retain independent charges until disposal; the caller keeps streams
and the borrowed root alive through completion and all surviving results.

InspectHeaders/InspectHeadersAsync scan through EOF with fixed8448-byte allowance
(8KiB scratch plus coarse parser state). No image/metadata-sized buffer is retained.
Seek opaque payloads when possible, otherwise drain; scan JPEG entropy framing.
Read-ahead/skipped bytes count toward input limits. Read EXIF entries individually,
without following thumbnails/other directories/external offsets. Report dimensions,
source representation, precision, alpha semantics and IFD0 orientation consistently.
Dimension/pixel/input/metadata/item limits apply without hypothetical decoded-array
limits. Check container order/lengths/profile/EOF; omit PNG CRC, JPEG table/entropy
and WebP pixel validation. Identify validates fuller containers; Decode validates
pixels. Async inspection uses ReadAsync and available synchronous seek.

EncodePng/Jpeg/WebpAsync use actual WriteAsync with ConfigureAwait(false), no worker
threads, synchronous caller writes/flush/seek or final ToArray. Stage PNG/JPEG and
lossless WebP privately; lossy WebP delivers prepared partitions. Writes are at most
64KiB. Admit actual staging capacity plus256B stream/64B chunk bookkeeping; growth
covers overlap. Keep each owner through actual writes even on cancellation/failure.
Unused tails are cleared before exposing array-backed memory. Governed calls bypass
uncharged idle pools. Otherwise output reuse has at most two arrays totaling4MiB.

Staging failure leaves caller output untouched. Delivery or synchronous encoding
failure/cancellation can leave a prefix; stage externally for atomic publication.
Keep samples/images stable and caller output alive until completion. The JPEG async
sample overload takes ReadOnlyMemory and retains direct gray/RGB/stride contracts.
No production filesystem/process capabilities are used.

See [resource admission](docs/resource-admission.md) for ownership and
[cleanup qualification](docs/cleanup-qualification.md) for current evidence.
