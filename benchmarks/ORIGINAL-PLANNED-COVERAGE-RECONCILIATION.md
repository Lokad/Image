# Original planned coverage reconciliation

The original finite scope explicitly contains **240 numeric + 17 unmeasured**
controls. Its numeric snapshot does not enumerate the 17; their recipes survive in
[the pre-cleanup coverage document](https://github.com/lokad/image/blob/39402e74e0f95c82ab209e5a950066a206c2af65/benchmarks/WARM-COVERAGE.md).
The later text/drawing omission audit searched numeric rows and benchmark sources;
that did not retire earlier planned operations. Current views sometimes have
other inputs, aliasing or operation boundaries. Functional checks and a passing
related algorithm are not performance proof for a different declared recipe.

**Keep the declared 289-path scope and every original name.** This reconciliation
adds no required path or workload grid and retires none. Image-view counts and the
current summary are subsets, not completion proofs. Resolve recipe aliases using
valid evidence before adding measurement; never replace a missing original boundary
with an easier operation. The following finite list is the inherited 17:

| Original required name | Distinct path / current action |
| --- | --- |
| `InspectHeaders/PngRgba8FreshSeekable` | Complete chunk/EXIF/EOF grammar without pixel/CRC decoding. Reconciled pass-stop:current `PngHeaders` has the exact218775B packet,fresh seekable grammar/EOF/owner boundary and identical native reference identity. 0.290585/0.579888us,upper.509395,568B/op;reuse [RCI1098](REFILL-CALL-INLINING.md). |
| `InspectHeaders/Jpeg420FreshStream` | [Batched entropy inspection](JPEG-BATCHED-ENTROPY.md) passes/stops:1.999/AVIO-headeradapter2.799us,upper.730193,8768B/op. Controlled actual-DLL pairs save47.63%/1.912us. Fresh opaque seekable source,full entropy/late metadata/EOF and independent9-field output remain inside. |
| `InspectHeaders/WebpLosslessRgba8FreshSeekable` | Complete RIFF and late-metadata framing through EOF. [Current header guard](JPEG-STUFFED-PAIRS.md) passes/stops.142106/AVIO-headeradapter.310231us,upper.471884,552B/op; full RIFF/late metadata/EOF/independent9-fields. Identify/probe remain different. |
| `FillRectangle/Rgba8Clipped` | [Original clipped fill](ORIGINAL-CLIPPED-FILL.md) passes/stops:request(-3,5,257,129),effective254x129. 1.194663/FFmpegpublicfill1.730184us,upper0.696649,0B/op. Exact current-package normal/scalar/fullcanvas/owner gates;zero production changes. |
| `FillRectangle/Rgba16Clipped` | [Original clipped fill](ORIGINAL-CLIPPED-FILL.md) passes/stops:request(-3,5,257,129),effective254x129. 2.183548/FFmpegpublicfill4.829109us,upper0.456739,0B/op. Exact current-package normal/scalar/fullcanvas/owner gates;zero production changes. |
| `CopyRegion/Rgba8Independent` | Original clipped 254×129 copy passes on immutable package `28fb64ab4529` at 1.575331/1.862436 µs against the FFmpeg adapter, upper 0.851870, 0 GC B/op ([ledger](final-package-benchmarks.json), `case136`). Frozen source (0,0), request 257×129, destination (-3,5). The [earlier copy round](SELF-ROW-STORAGE.md) records the gap on its original producer. |
| `CopyRegion/Rgba16Overlapping` | [Shared self-row state](SELF-ROW-STORAGE.md) passes/stops5.186/FFmpeg5.602µs,upper.930762,0B;controlled actual-DLL20.26%/1.110µs gain. Frozen257×129,(1,1),forward/reverse snapshot/checkpoint/ownership boundary unchanged. |
| `Composite/Rgba8StraightAlpha` | [Original clipped composite](ORIGINAL-CLIPPED-COMPOSITE.md) passes/stops78.567/retained FFmpegoverlay526.201us,upper.159260,0B/publicoperation. Fullsourceat(-3,5),effective510x254;reset excluded before EVERY operation both. Exactmanaged and approved nativeRGB/alpha/untouched/zero-alpha gates. Graph preparation outside,not one-shot. |
| `Composite/Rgba16Overlapping` | Rational word arithmetic plus alias traversal. [Original self-composite](ORIGINAL-WORD-COMPOSITE.md) now passes/stops649.772/ImageSharp716.960us,upper.976714. Reference includes necessary sourceClone+Rgba64SrcOver+Dispose;0/26800GC B/op. Full-word exact/native/owner gates pass;no production change. |
| `TextLayout/OriginalUnicode` | [Complete authored layout](AUTHORED-FONT-COMPARISONS.md) passes/stops2.585/117.234µs,upper.023246,5592/367584B;explicitly source-corrected public ImageSharp integration,all nine layout calls/tab adaptation and owned disposal included. |
| `TextLayout/PackedBmp` | [Complete authored cmap4 layout](AUTHORED-FONT-COMPARISONS.md) passes/stops2.610/120.738µs,upper.022768,5088/385016B;frozen short-loca/packed/implied-point inputs,corrected reference delta mapping. |
| `TextLayout/CompositeTransforms` | [Complete authored compound layout](AUTHORED-FONT-COMPARISONS.md) passes/stops2.604/112.021µs,upper.023809,6152/366960B;independent FreeType geometry,explicit point-attachment/metric reference corrections. |
| `TextRender/Rgba8Clipped` | [Original complete layout+coverage+drawing](TEXT-CONDITIONAL-POLYGON-SCRATCH.md) passes/stops41.223/224.429µs,upper0.299789,41064/417912B;peroperation resets,full16-bit samples,exact managed frame and approved visual gates unchanged;explicitly source-corrected reference. |
| `TextRender/Rgba16Clipped` | [Original complete layout+coverage+drawing](TEXT-CONDITIONAL-POLYGON-SCRATCH.md) passes/stops38.190/225.899µs,upper0.294813,41064/417912B;peroperation resets,full16-bit samples,exact managed frame and approved visual gates unchanged;explicitly source-corrected reference. |
| `TextLayout/OpenTypeArabicMarksDigits` | Joining,bidi,GSUB and mark positioning. Current TextLayoutOpenTypeArabic pass/source-valid evidence; keep frozen scenario and public API multiplicity. |
| `TextFit/OpenTypeConstrained` | Thirty fitting trials and owned winner. Current TextFitOpenType pass/source-valid evidence; retain all trials and winner reconstruction. |
| `TextLayout/OpenTypeDefaultVariation` | [Authored default variation](DEFAULT-VARIATION.md) passes/stops:3.551/ImageSharp public integration20.357us,upper.184945,3608/58496B. Exact glyph/cluster/line/position/owned outputs;four reference public calls and disposal inside,fonts outside. |

The three tiny-face layout controls retain `AV g Á\u0301 \uE000\nAV\tg AV`,
size16,wrap64,line spacing1.25; the original byte/word rendering controls retain
513×259 canvases and position(-.25,1.5),with font preparation outside but layout and
drawing inside timing. Default variation uses the existing authored two-glyph
contextual program,GDEF regions,signed deltas and default-zero adjustment. Use
approved public reference pipelines and existing quality/layout/full16-bit gates;
no private face becomes a fixture and no expanded face/script/settings grid.

All 17 inherited controls have passing scoped evidence. The original clipped
byte copy is closed by measurement of that exact operation on package
`28fb64ab4529`. The ledger records the final package qualification.

[AFP1134](AUTHORED-FONT-REFERENCE-PREFLIGHT.md) retains the superseded reference
failures. [ATS1142/AFT1143](AUTHORED-FONT-COMPARISONS.md) separates the defects,
corrects explicitly labeled tooling and independently confirms all five complete
operations. All original fixture bytes, managed observations/frames, exact and
visual gates remain unchanged. Windows/Linux normal/scalar and retained-owner/root
cleanup pass. Stock and corrected identities, patches and minimal reproductions
remain retained; existing default-variation evidence keeps its original reference.

Current measurements and exact boundaries are linked in the table. Source-valid
PNGheader,Arabic/fitting and unchanged-path qualification retain their producer
identities. New byte-composite uses a retained FFmpeg graph/static source,not
one-shot graph preparation;reset is excluded before EVERY operation both sides.
All managed samples remain exact;case-specific native bounds stay explicit.

[Batched entropy inspection](JPEG-BATCHED-ENTROPY.md) now qualifies the changed JPEG
path with 6,749 full/live checks per OS and SAME actual package/all three examples.
Unchanged drawing/font paths retain their explicitly identified producers/costs.
Normal/scalar current-package drawing checks and independent confirmations remain;
no broad campaign or favorable repetition. Allocation, admission, prepared retention
and physical/native/JIT peaks are separate. All 289 required paths stay in scope.

Completed-round history remains in [word-overlap](ORIGINAL-WORD-OVERLAP.md),
[word dispatch](WORD-ALIAS-DISPATCH.md), [word compositing](ORIGINAL-WORD-COMPOSITE.md),
[complete-header references](BOUNDED-HEADERS-CURRENT-REFERENCE.md),
[JPEG acquisition](JPEG-STUFFED-PAIRS.md), [clipped fills](ORIGINAL-CLIPPED-FILL.md),
[clipped copy](ORIGINAL-CLIPPED-COPY.md) and [clipped composite](ORIGINAL-CLIPPED-COMPOSITE.md).
Their cited raw archives and reproducibility remain; superseded costs are history.

[Evidence](original-planned-coverage-reconciliation-evidence.json) retains the
original sources, 17 names, precise source/ownership/reference invalidation proofs
and current providers. Publication stays postponed; independent work continues.
