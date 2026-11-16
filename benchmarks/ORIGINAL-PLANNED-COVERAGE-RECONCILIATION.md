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
| `InspectHeaders/Jpeg420FreshStream` | Complete marker/stuffing scan through entropy and late metadata. [Stuffed-pair acquisition](JPEG-STUFFED-PAIRS.md) saves31.743% in controlled pairs; current3.769/AVIO-headeradapter2.835us,upper1.367175,8768B/op remains a gap. Fresh opaque seekable source/full scan/independent9-field output. |
| `InspectHeaders/WebpLosslessRgba8FreshSeekable` | Complete RIFF and late-metadata framing through EOF. [Current header guard](JPEG-STUFFED-PAIRS.md) passes/stops.142106/AVIO-headeradapter.310231us,upper.471884,552B/op; full RIFF/late metadata/EOF/independent9-fields. Identify/probe remain different. |
| `FillRectangle/Rgba8Clipped` | [Original clipped fill](ORIGINAL-CLIPPED-FILL.md) passes/stops:request(-3,5,257,129),effective254x129. 1.194663/FFmpegpublicfill1.730184us,upper0.696649,0B/op. Exact current-package normal/scalar/fullcanvas/owner gates;zero production changes. |
| `FillRectangle/Rgba16Clipped` | [Original clipped fill](ORIGINAL-CLIPPED-FILL.md) passes/stops:request(-3,5,257,129),effective254x129. 2.183548/FFmpegpublicfill4.829109us,upper0.456739,0B/op. Exact current-package normal/scalar/fullcanvas/owner gates;zero production changes. |
| `CopyRegion/Rgba8Independent` | [Original clipped byte copy](ORIGINAL-CLIPPED-COPY.md) confirms1.941619/FFmpegpubliccopy1.901433us,interval1.017439–1.026874,0B/op gap. Freeze unspecified source origin(0,0);request257x129,dst(-3,5),effective254x129. Peroperation clipping/fullcanvas/source/normal/scalar gates;no production change. |
| `CopyRegion/Rgba16Overlapping` | Word memmove and reverse traversal with one owner. OWC1125 establishes the initial gap;current [word-alias dispatch](WORD-ALIAS-DISPATCH.md) reduces it to5.576/FFmpeg5.077us,upper1.104081,0B/op. Still open. |
| `Composite/Rgba8StraightAlpha` | Clipped straight-alpha source-over. Current separate-owner composition passes at(17,19); original(-3,5) recipe still needs reconciliation. |
| `Composite/Rgba16Overlapping` | Rational word arithmetic plus alias traversal. [Original self-composite](ORIGINAL-WORD-COMPOSITE.md) now passes/stops649.772/ImageSharp716.960us,upper.976714. Reference includes necessary sourceClone+Rgba64SrcOver+Dispose;0/26800GC B/op. Full-word exact/native/owner gates pass;no production change. |
| `TextLayout/OriginalUnicode` | Authored cmap12/long-loca,quadratic extrema and compound accent. Unmeasured original tiny-face complete layout; DejaVu timings do not cover this declared input. |
| `TextLayout/PackedBmp` | Authored cmap4/short-loca,packed/repeated flags and implied points. Unmeasured distinct required mapping/outline path. |
| `TextLayout/CompositeTransforms` | Authored matrices,point attachment and inherited metrics. Unmeasured distinct required compound outline path. |
| `TextRender/Rgba8Clipped` | Original authored face,layout+coverage+borrowed-byte drawing. Original layout+draw boundary remains unmeasured; prepared-layout drawing is different. |
| `TextRender/Rgba16Clipped` | Original authored face,layout+full-word coverage/composition. Same missing original complete operation; preserve full16-bit gates. |
| `TextLayout/OpenTypeArabicMarksDigits` | Joining,bidi,GSUB and mark positioning. Current TextLayoutOpenTypeArabic pass/source-valid evidence; keep frozen scenario and public API multiplicity. |
| `TextFit/OpenTypeConstrained` | Thirty fitting trials and owned winner. Current TextFitOpenType pass/source-valid evidence; retain all trials and winner reconstruction. |
| `TextLayout/OpenTypeDefaultVariation` | Authored contextual GSUB plus GDEF region/delta/default-zero data. Unmeasured declared default-instance path; private inputs are not fixtures. |

The three tiny-face layout controls retain `AV g Á\u0301 \uE000\nAV\tg AV`,
size16,wrap64,line spacing1.25; the original byte/word rendering controls retain
513×259 canvases and position(-.25,1.5),with font preparation outside but layout and
drawing inside timing. Default variation uses the existing authored two-glyph
contextual program,GDEF regions,signed deltas and default-zero adjustment. Use
approved public reference pipelines and existing quality/layout/full16-bit gates;
no private face becomes a fixture and no expanded face/script/settings grid.

OWC1125 executes the first missing boundary on its actual immutable package:
original RGBA16 self-overlap,257×129,shift(1,1). The prose did not fix the absolute
origin; freeze(0,0)→(1,1),then reverse in both timed totals. It independently shows
7.010/FFmpeg5.092µs,upper1.379847,0managedB/op. Full-word snapshot/untouched pixels,
normal/scalar,refusal/cancellation/recovery and equivalent borrowed ownership pass.
No production build or broader package campaign is needed for this measurement.
[Word-alias dispatch](WORD-ALIAS-DISPATCH.md) subsequently reduces this gap with a
controlled16.697%gain;current5.576/5.077us,upper1.104081 remains open. Both reports
retain producers,raw actuals and source identities;no historical-clock gain is inferred.

COV1127 resolves the original PNG-header name using existing evidence. The bridge
receipt names `InspectHeaders/PngRgba8FreshSeekable`; its frozen packet SHA-256
`4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f`
and all three native source/build/worker identities match the RCI1098 provider.
The reference is explicitly FFmpeg AVIO/descriptor APIs plus the original bounded
header/EXIF adapter,not public PNG-parser performance. Current typed nine-field
metadata, fresh source/workspace/descriptor acquisition/release, opaque IDAT seek,
late metadata and EOF are included;CRC/pixels are excluded equally. RCI1098's
shared reader and the public header entry remain;JID changes only JPEG with
outside-range reconstruction,and POC changes Probe wrappers only. Eight scoped
source pins still match. Stop this pass;zero new builds,measurements or broad
checks. Reuse the current platform/package qualification for unchanged paths.
The evidence records precise acquisition/grammar/factory/refill/owner/reference
changes that would invalidate reuse;related JPEG/font/copy edits do not.

WAC1128 independently confirms the original full-word self-composite at(1,1),
effective512×258. Direct ImageSharp self-drawing fails snapshot semantics;the
explicitly labeled sourceClone+Rgba64SrcOver+Dispose reference includes that
necessary work in timing. Both restore the canvas outside each draw. The current
upper.976714 passes/stops,with exact managedBigInteger pixels and unchanged
reference max1/RMSE.25/untouched/zero-alpha gates. GC0/26800B is distinct from
at-least1062936B reference snapshot payload and unmeasured retained allocator
capacity. Existing independent-source costs did not substitute for this boundary.
One successful tooling build after a pre-check compile correction;no production,
existing harness,Windows,broad or package builds. Reuse current qualification.

HBC1129 freezes the existing complete-JPEG guard input(65,163B,SHA5d3a88c6)
and original lossless WebP packet(533,100B,SHAa4ed2454). Independent complete
header comparisons use the same opaque seekable/EOF/descriptor boundaries and
labeled FFmpegAVIO/descriptor APIs plus original bounded structural/EXIF adapters.
JPEGupper1.994260 is a demonstrated gap;WebPupper.456690 passes/stops. Twenty
same-packet controls and bothOS normal/scalar/owner/cancel/refusal/recovery pass.
This adds no image or required path;source/worker/packet identities and bounded
memory differences are explicit. Old scoped JPEG-guard clocks are not substituted
for standalone reference evidence. Next JPEG coalescing hypothesis follows one
current-process profile;its sample counts/instrumented clocks are not gain claims.

SMP1130 accepts31.743%/1.753749us complete-JPEG savings with unchanged allocation,
then independently refreshes current JPEG/WebP headers and async JPEG dimensions.
JPEGheaderupper1.367175 and asyncprefixupper1.133673 remain gaps;WebPupper.471884
passes/stops. The asyncprefix2.698ns/1.255% regression is explicit. Full6747eachOS,
normal/scalar ownership guards and SAMEactualpackage/all3examples pass. Older
HBC1129costs above remain baseline history, not current production proof.

CLF1131independently qualifies both original254pixel clipped-fill tails against
FFmpegpublicfill:byteupper.696649 and wordupper.456739,0B/op. Both pass/stop with
exact normal/scalar current-package samples. Existing256pixel costs remain
separate. No production change,new image/path or broad/package campaign. Original
byte-copy/composite and six authored-font boundaries remain unmeasured.

CCB1132resolves the original clipped-copy measurement as a small2.1%/40ns gap,
not another optimization campaign. Complete clipping/borrowed-source ownership
and exact normal/scalar pixels pass on the unchanged package;zero production/
package builds. Only original byte-composite and six authored-font boundaries
remain unmeasured;earlier missing-state paragraphs retain checkpoint history.

The audit is read-only and creates no new coverage machinery. [Evidence](original-planned-coverage-reconciliation-evidence.json)
pins both historical sources and all 17 names. Existing qualifying paths stay
stopped; uncertainty and independent work remain distinct. Publication stays postponed.
