# Lokad.Image supported contract

Lokad.Image 0.2.0 is an independently implemented, safe C#/.NET 10 still-image
library. The production package has no dependencies, unsafe code, native bindings,
subprocess codecs or vendored sources. Published 0.1.0 packages are immutable.

## Formats

| Format | Decode | Encode |
| --- | --- | --- |
| PNG | All legal static color/depth combinations, palette/tRNS, five filters, Adam7 | Noninterlaced grayscale, RGB, gray-alpha and RGBA at 8/16 bits; stored, fastest, optimal and smallest compression |
| JPEG | 8-bit Huffman baseline/progressive gray, RGB, YCbCr, CMYK and YCCK; 4:4:4, 4:2:2, 4:2:0 and three-component 4:1:1/4:1:0 | Baseline gray/YCbCr, 4:4:4/4:2:2/4:2:0, quality, restart intervals and explicit alpha flattening |
| WebP | Still VP8/VP8L, extended containers, raw/compressed ALPH | Lossless VP8L or lossy VP8, alpha, quality and effort controls |

GIF, animation (including APNG), arithmetic/lossless/12-bit JPEG, progressive JPEG
encoding, indexed/Adam7 PNG encoding and metadata round-tripping are unsupported.
ICC color conversion is not implemented. EXIF reorientation is opt-in; no codec
silently applies color management. [Codec parity](codec-parity.md) names tested
reference controls; it does not promise byte identity for arbitrary inputs/settings.

JPEG quality is 1–100 (default 85), sampling defaults to 4:2:0. Color quality maps
to reference qscale `1 + (100 - Quality) * 30 / 99`; grayscale has its own quantizers.
Quality 100 remains lossy. Equal quality numbers across encoders are not equivalent.
Transparent image input requires explicit `BackgroundRgb`; flattening and 16→8
normalization use one nearest-integer rounding. Caller gray/RGB samples are opaque.

WebP defaults to lossless, effort 4 (range 0–6). Lossy quality is 1–100, default 85;
100 remains lossy. Lossless ignores quality and preserves alpha and hidden RGB.
RGBA16 normalizes once using `(sample + 128) / 257`; JPEG/WebP output is 8-bit.
No encoding decision recovers from a refused shared admission by changing strategy.

## Pixels, editing and ownership

`ImageBuffer` owns tightly packed, top-to-bottom, straight-alpha RGBA storage:
four bytes/pixel at 8 bits or eight at 16 bits. `GetRow16` exposes native-endian
unsigned samples. New buffers are zeroed; live results have independent storage.
Dispose images, fonts and layouts after their final users, including borrowed row
spans. Previously exposed spans cannot be revoked by safe C# and must not be used
after disposal. Callers synchronize access to mutable inputs/canvases.

Clone, crop, flips, quarter turns, transpose and all eight EXIF orientations
preserve every sample, including hidden RGB, and return independent results.
Orientation remains pending until `AutoOrient` or opted-in decoding applies it,
then resets to Normal. Crop rectangles must fit; centered crop floors leftover
odd pixels, while clamped crop moves the origin without changing requested size.

Resize supports nearest, bilinear, Catmull-Rom bicubic and Lanczos3 at both depths.
Fit preserves aspect ratio, rounds halves away from zero with a one-pixel minimum,
and can disallow enlargement. Sample centers map to
`(destination + 0.5) * sourceSize / destinationSize - 0.5`; edges extend by clamping.
Reduction widens the filter. Filtering uses encoded sample space and premultiplied
alpha, then unpremultiplies and rounds halves away from zero. Filtered zero-rounded
alpha has zero RGB; identity resizing preserves hidden RGB. Lanczos3 uses
`sinc(x) * sinc(x/3)` within radius three, normalized Q30 weights and exact final
integer rounding. SIMD and portable paths preserve the same defined pixels.

`ImageDrawing` mutates coded canvas coordinates without applying orientation.
Rectangle fills replace samples; region copies require matching depths and an
in-bounds source rectangle. Destinations clip. Source-over compositing supports
mixed depths and rounds only final samples; zero source alpha leaves all canvas
samples unchanged. Self-copy/compositing reads original samples without a full-frame
snapshot. Invalid geometry/admission fails before mutation; cancellation may leave
a completed prefix. `RgbaColor` holds 16-bit samples; `From8` expands by 257.

## Caller JPEG samples

`DecodeJpeg` writes borrowed interleaved Gray8/RGB8/CMYK8 spans without mandatory
RGBA or encoded-input copies. Stride zero means packed; positive strides must
cover a row. Padding/suffix bytes are untouched; source/destination must not
overlap. Failures may leave a completed pixel prefix. Encoding accepts borrowed
Gray8/RGB8 samples, including async `ReadOnlyMemory`; gray overrides sampling.

`SourceColorSpace` explicitly overrides Auto's marker/component interpretation.
Canonical CMYK uses 0 for no ink, 255 for full ink. Auto inversion inverts Adobe
CMYK/YCCK and leaves untagged CMYK unchanged; `None`/`Invert` override it. YCCK
converts the inverted CMY components while keeping black separate. RGB conversion
is a device-color approximation, not ICC conversion; JPEG K is never alpha.
CMYK output requires four source components; CMYK encoding is unsupported.
Owned/caller outputs share the fixed-point [inverse policy](jpeg-inverse-rounding.md).
Opted-in orientation writes directly into the oriented caller layout.

## Supplied fonts and text

Load standalone TrueType `glyf` faces; Unicode `cmap` 4/12, simple/composite quadratic
outlines and horizontal metrics are supported. No system-font lookup, synthetic
style, font collection, CFF/WOFF, hint execution or variation-axis selection is
provided. Select an actual supplied regular/bold/italic face. Unsupported selected
features fail explicitly; malformed structure is a data failure. Missing characters
use glyph zero. Loading validates structure; outlines are checked as used.

`TextLayout.Create` measures and retains the exact placements/outlines later used
by `DrawText`. Font size is pixels per em. Logical layout is the default;
`TextShaping.OpenType` enables supported Latin/Hebrew/Arabic shaping and bidi layout.
Supported lookup boundaries and the bounded font comparison set are documented in
[text compatibility](text-compatibility.md); this is not a general font-engine claim.

Wrapping is greedy, removes separators at automatic breaks and preserves explicit
line breaks. Logical mode breaks long words between scalars; OpenType mode preserves
grapheme/ligature clusters, which may overflow. Line advance uses `hhea` metrics
times `LineSpacing`; start/center/end alignment follows paragraph direction.
`LayoutBounds` covers line boxes/advances, `InkBounds` actual quadratic extrema,
and `Bounds` their union, including overhangs. Empty text has zero lines/bounds.

`TryFit` searches a finite descending font-size grid and returns the largest
fitting completed layout or false/null. Position it at `(-Bounds.X, -Bounds.Y)`.
Resource refusal, cancellation and unsupported features terminate fitting rather
than trying a smaller size. Layouts remain usable after source-font disposal.
Rendering clips to the canvas, uses unhinted area coverage and source-over at both
depths; it allocates no full-frame mask. Rasterization differences have explicit
comparison tolerances, not a promise of identical antialiasing.

## Inspection, streams and failures

`ImageInfo` describes encoded dimensions, orientation, sample/index `BitDepth`,
`SourceColor`, `SourceAlpha` and `SourceComponentCount`, independently of normalized
RGBA. PNG distinguishes indexed/gray/RGB and explicit alpha versus palette/color-key
`tRNS`. Ambiguous JPEG source models remain Unknown. VP8L's alpha representation
exists even when its header hint leaves `HasAlpha` false. See [source metadata](source-metadata.md).

Identify validates containers through EOF without decoding all pixels. Decode also
validates pixel coding. `InspectHeaders` scans through EOF using fixed 8,448-byte
workspace, without retaining image/metadata-sized buffers; it checks framing and
headers but omits PNG CRC, JPEG table/entropy and WebP pixel validation. All four
sync/async identification/inspection APIs agree on metadata for valid inputs.

Development 0.2.1 adds `Probe`/`ProbeAsync`: select dimensions, source representation,
transparency, orientation or animation; stop once those facts are established.
Null result fields remain unknown. Default dimension probing stops before pixel
payloads. Probing validates only the examined prefix; it does not establish full
container, CRC or pixel validity. [Source metadata](source-metadata.md) describes
acquisition bounds and stream positioning.

Read operations default to strict EXIF header/IFD0 orientation checks. Explicit
`ImageMetadataPolicy.IgnoreInvalid` tolerates unusable optional EXIF and selects
the last usable orientation, preserving required container/pixel checks and limits.
See [metadata policy](source-metadata.md); reorientation remains opt-in.

Caller streams remain open. Async APIs use actual asynchronous reads/writes,
without worker-thread offloading; CPU codec work is synchronous. Identify/Decode
stream buffering is input-bounded and avoids a final exact-length copy. Async
encoding retains staging or prepared partitions until actual writes finish.
Preparation failure leaves output untouched; delivery/synchronous failure may leave
a prefix. Stage externally when atomic publication is needed.

Defaults: 32,768 pixels/side, 32 million pixels, 64 MiB input/output each, 512 MiB
working storage, 4 MiB metadata and 100,000 container/EXIF items. Codec and managed
buffer ceilings also apply. Local limits count borrowed input; shared admission
charges owned capacity/copies. Neither measures process RSS. There is no CPU quota.

Pass one borrowed `IMemoryBudget` through `ImageLimits.MemoryBudget` for nested and
concurrent admission. Reserve before allocation, including replacement overlap;
retained results stay charged until explicit disposal. Refusal is terminal, never
a retry/wait/fallback. Async storage stays charged through actual users, including
failure/cancellation. The [memory contract](resource-admission.md) defines hooks,
disposal, zero/negative semantics and coarse-accounting boundaries.

`ImageDataException` identifies malformed data; `UnsupportedImageException` exposes
Feature/recognized Format; `ImageLimitException` exposes Limit and Operation or
SharedBudget Scope. Shared refusal reports WorkingBytes. Cancellation preserves the
supplied token; allocation failure remains distinct. Argument errors use standard
argument exceptions. [Usage](usage.md) and [migration](migration.md) cover public APIs.
