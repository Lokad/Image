# Material warm coverage

The full objective remains independent quiet single-CPU warm performance with a
conservative ratio upper bound ≤1× for every named material required path, plus
functional, memory, compression and packaged-consumer correctness. Startup and
concurrency are excluded. Compression correctness precedes CPU optimization.
The published 0.2.0 snapshot is retained as the baseline; current evidence is
reconciled in the summary below.

[Current Lanczos3 byte reduction](CURRENT-LANCZOS3-REDUCE8-COST.md) independently
confirms the existing513×259RGBA8→256×129 path at1.500× / upper1.511,
138,451B/op. Exact managed/native pixels, source/independent ownership and quiet
guards pass. This unchanged-path cost confirmation reuses current Windows/Linux
and actual-package qualification. One small selector build, no production,
harness/native build or new control. No general Lanczos CPU-pass claim follows.

[Current Lanczos3 word reduction](CURRENT-LANCZOS3-REDUCE16-COST.md) independently
confirms the existing513×259RGBA16→256×129 path at1.585× / upper1.666,
138,439B/op. Exact independent managed pixels, unchanged native max1/RMSE.25,
source/owners and quiet guards pass. One selector build reuses unchanged
production/harness/native/oracle/full/package evidence; no new cases.

[Current Lanczos3 byte enlargement](CURRENT-LANCZOS3-ENLARGE8-COST.md) independently
confirms the existing513×259RGBA8→1027×519 path at1.033× / upper1.038,
319,038B/op. It remains a gap. Exact independent pixels/native bounds/source/
owners/quiet pass. Zero builds and no repeated qualification; no controls added.

[Current Lanczos3 word enlargement](CURRENT-LANCZOS3-ENLARGE16-COST.md) independently
confirms the existing513×259RGBA16→1027×519 path at1.267× / upper1.453,
4,583,983B/op. Exact independent pixels/native bounds/source/owners/quiet pass.
Zero builds/repeated qualification. All four existing Lanczos costs are now
independently current, with unchanged source; all remain gaps.

Closed Lanczos experiments retain their rejected sources, raw samples and
restoration in linked reports: [packed premultiplication](LANCZOS3-PACKED-PREMULTIPLICATION-REJECTED.md),
[vertical batching](LANCZOS3-FOUR-TAP-VERTICAL-REJECTED.md),
[word spans](LANCZOS3-WORD-SPAN-HOISTING-REJECTED.md),
[paired normalization](LANCZOS3-PAIRED-NORMALIZATION-REJECTED.md),
[reciprocal normalization](LANCZOS3-RECIPROCAL-NORMALIZATION-REJECTED.md),
[coefficient symmetry](LANCZOS3-COEFFICIENT-SYMMETRY-REJECTED.md) and
[output-resident source products](LANCZOS3-OUTPUT-ROW-PRODUCTS-REJECTED.md) and
[adjacent-window source products](LANCZOS3-ADJACENT-SOURCE-PRODUCTS-REJECTED.md).
The original Lanczos implementation is restored; these trials change no accepted cost,
required case or applicable full/package qualification.

[Current effort-6 VP8 diagram](CURRENT-VP8-EFFORT6-COST.md) independently
confirms1.401× / upper1.415,754,344B/op, unchanged exact6,818Bpacket. All12actuals,
native brackets/CLI pixels/source/owners/quiet pass. Zero builds; unchanged
full/package proof reused. It remains a gap, without a causal comparison against
the older short screen. No required case added or removed.

[Current RGBA16 lossless WebP](CURRENT-VP8L-RGBA16-COST.md) independently
confirms1.341× / upper1.431,29,696,928B/op and exact183,812Bpackets. All12actuals,
native/sample/alpha/hiddenRGB/source/owner/quiet gates pass. Zero builds and
unchanged qualification reused; no causal comparison against older screens.
It remains a gap; required controls/settings stay unchanged.

[Current photograph VP8L](VP8L-SHARED-COLOR-MASK-REJECTED.md) independently
confirms1.360× / upper1.397,20,418,440B/op and exact160,032Bpackets. A shared
color-mask candidate fails confirmation; original source/exact binaries are
restored. All native/pixel/source/owner/quiet gates pass; unchanged full/package
proof is reused. The photograph remains a gap; no required case changes.

[Output-resident word rows](WARM-BICUBIC-OUTPUT-ROW-CACHE.md) accepts26.2–29.3%
lower TightCross16 mean elapsed in both predeclared A/B/B/A time directions,
with separate filtered/unfiltered and reference-normalized confidence envelopes.
Current conservative cost1.232× / upper1.396,1,063,419B/op remains a gap. Only
TightCross16/TightHorizontal16 costs invalidate; both are refreshed. Exact pixels,
native/source/owners/quiet,5,080checks/OS and SAME actual package/three examples
pass. Reuse the exact existing build/scalar proof; no core/harness/native rebuild,
new image or profile. The prior COR1 result keeps its unqualified status.

[Bounded greedy cancellation polling](PNG-FAST-CANCELLATION-POLLS-REJECTED.md)
fails its 5% screen gate. The original source and exact qualified binaries are
restored; accepted PNG costs, package proof and required cases are unchanged.
No variants or independent timing confirmation follow.

The superseded [VP8-plus-alpha validation cost](CURRENT-VP8-ALPHA-VALIDATION.md) independently
confirms the existing borrowed-span control at1.523× / upper1.697,770,092B/op.
Complete coding, nine-field descriptor, source/surviving ownership and quiet gates
pass, with12actuals and unchanged native/settings/input/boundary. Zero builds or
repeated qualification; the original producer header is restored exactly. This
replaces its uncertain screen without a causal gain/regression or new case.

[VP8 token-local state](VP8-TOKEN-LOCAL-STATE-REJECTED.md) fails its 5% screen
gate. Original source/exact binaries/package are restored; current validation
costs and required paths stay. No variants or independent confirmation follow.

[VP8 fixed-alignment arithmetic](VP8-FIXED-ALIGNMENT-WINDOW-REJECTED.md) also
fails its 5% screen gate. Its independent exact algebra and cursor checks pass,
but no complete-operation gain follows. Both source files/exact binaries/package
are restored; no width/inlining variants or repeated qualification.

[Direct VP8 token decisions](WARM-VP8-DIRECT-TOKEN-DECISIONS.md) accept7.20–8.12%
less elapsed in both predeclared counterbalanced time orders, with separate raw
and reference-normalized confidence envelopes. Current ValidateVp8Alpha1.351× /
upper1.382,770,092B/op remains a gap; StreamNative8 has a current1.274×/upper2.005
short,1,966,583B/op and exact frozen pixels. BothOS5,082full checks,32internal/
350publicscalar andSAMEactualpackage/3examples pass. Same immutable source/builds
through all measurements/qualification; no counterbalanced rebuild or retuning.
Other buffered VP8 normal/cancelable/delivery/opaque/simple-filter/alpha costs
need applicable current evidence;scalar short inputs and unrelated paths retain
unchanged proof. No required case added or removed. Earlier VND1pair stays
unqualified; this new bounded block supplies accepted evidence.

[Current resident VP8 costs](CURRENT-VP8-DECODER-COSTS.md) reconcile four existing
paths after VNB1: normal-filter opaque ordinary/live-token1.170/1.186×,
simple-filter opaque1.253×, resident VP8+ALPH1.246×. Conservative uppers are
1.210/1.193/1.280/1.273; all remain confirmed gaps. Exact pixels, source/owners,
12actuals/native brackets and both quiet guards pass. Zero builds/new controls;
unchanged full/scalar/SAME actual package qualification is reused. These are
current costs, without a causal comparison against older producers. The four
named uncertainties are resolved; other affected decoder boundaries still need
applicable evidence before CPU-pass claims. Prior records remain historical.

[Shared scan CRC reuse](WARM-PNG-SCAN-CRC-REUSE.md) accepts28.7% less independent
storedRGBA16 decode elapsed, with unchanged8,568B/op and exact frozen packet/
pixels. Current0.750× / upper0.760 passes and stops. BothOS5,082full/live,
117focused normal/scalar,452publicscalar andSAMEactualpackage/3examples pass on
the same immutable build; zero harness/native/selector builds or new controls.
All four displayed validators are independently current: word span/async pass
with uppers0.955/0.843; indexed Adam7 remains a1.366× gap; byte span remains
confidence-open at0.986× / upper1.020 after one bounded final confirmation.
No more timing or code retuning follows that confidence result in this round.

This changes the scan checksum selected by57existing controls. The evidence names
47resident decode cases,4seekable stream/decode-composition cases,1nonseekable
async decode,1caller-root two-retained-output operation and4validators; all were in the original inventory. This round
refreshes5costs, leaving52affected costs to reconcile on this qualified build.
Their older CPU results are prior-production; functional/native/oracle proofs
remain. No required case added/removed. PNG encoding and metadata-only Identify/
inspection/probing, other codecs and editing/text over decoded buffers retain
valid unchanged evidence. Do not confuse older2× successes with current1× proof.

[Current two-retained-output PNG cost](CURRENT-PNG-RETAINED-ROOT-COST.md)
independently confirms the existing caller-root boundary at 1.237× / upper 1.244,
2,574,629 B/op and 2,306,560-byte admission peak. Both complete decodes, overlap
and cleanup remain timed; exact samples/owners/charges/refusal/cancellation and
quiet gates pass. Zero builds or repeated qualification. One affected cost is
resolved after scan CRC reuse; 51 remain. It is a gap without a causal comparison
against older producers. No required case added or removed.

[Caller-root PNG row selection](WARM-PNG-CALLER-ROOT-ROWS.md) supersedes
the retained-root CPU cost: 3.13% less independent elapsed, 36.37% fewer allocated
bytes (1,638,183 B/op) and 1,766,641-byte admitted peak, with exact ownership and
cleanup. Current ratio 1.201 / upper 1.204 remains a gap. The changed shared Decode
body affects 53 existing boundaries; current stored16 is independently refreshed
at 0.753 / upper 0.758 and stops. Four Scan-only validation paths retain PCF1 costs.
51 named decode/stream/composition costs remain. Full 5,082/OS, 225 focused public
normal/scalar per OS and SAME actual package/three examples pass. No new cases.

[Current resident RGBA PNG costs](CURRENT-PNG-RGBA-DECODE-COSTS.md) resolve two
more existing cases without production rebuilds: RGBA8 1.131 / upper 1.166,
4,472 B/op is a gap; RGBA16 0.999 / upper 1.006, 8,568 B/op remains confidence
open. Both have 12 actuals, exact pixels/ownership and quiet guards. No retiming
or pass claim from mean alone. One generic cached-harness selector is reusable.
49 affected costs still need refresh, with current functional/package proof reused.

The same [resident PNG cost report](CURRENT-PNG-RGBA-DECODE-COSTS.md) now adds
four existing predictor/depth paths with zero builds: Paeth16 upper0.962 passes
and stops; Paeth8 interval0.989–1.031 stays confidence-open without retiming.
Average8/16 confirm1.143/1.180, uppers1.154/1.189,4472/8568Bop. All12actuals,
exactpixels/owners and quiet guards pass. Average16's older worker record is
reconciled explicitly to the same existing current reference settings/boundary.
45 costs remain unrefreshed; no case added, no passing/uncertain control retuned.

[Average SIMD reconstruction](WARM-PNG-AVERAGE-SIMD.md) now supersedes both
scalar gaps:20.3%/22.5%less independent elapsed, unchanged4472/8568Bop. Both
Average8/16 pass uppers0.916/0.918 and stop. Earlier fast-return kernels stay
unchanged; indexed validation is refreshed at1.359/upper1.369,1168Bop with exact
descriptor/owners and restored proof. Full5088/OS,11kernel+225public normal/scalar
andSAMEactualpackage/3examples pass. One corebuild/OS,zero harness/native/selector
builds.45costs remain unrefreshed; no case added or removed.

[Current Sub/Up PNG costs](CURRENT-PNG-SUB-UP-COSTS.md) resolve four more existing
cases with zero builds: Sub8/16 uppers0.728/0.413, Up8/16 uppers0.767/0.818 pass
and stop,4472/8568Bop. All12actuals,exact pixels/owners/native andquiet gates pass.
One immutablePAV1build/selector is reused; no qualification/package repeats.
41affected costs remain unrefreshed. No case added or removed.

[Current word PNG costs](CURRENT-PNG-WORD-DECODE-COSTS.md) resolve four more
existing cases with zero builds:RGB16/gray-alpha16/gray16 uppers0.996/0.851/0.739
pass and stop;Adam7RGBA16 remains1.346/upper1.362,23616Bop. Exactpixels/owners/
native/quiet all12actuals pass. Current diagnostic identifies47.1%BCL inflation
versus1.7%sparsecopy; no microcopy trial.37costs remain unrefreshed, no case added.

[Adam7 framed-output reuse](WARM-PNG-ADAM7-FRAMED-OUTPUT.md) supersedes the
seven-pass gap:43.8%less elapsed,0.758/upper0.761,23240Bop PASS/STOP. Additional
admitted filtered capacity1063423B uses the existing bounded pool; shared-root
and tight-ceiling behavior remains. Full5094/OS,11kernel+231publicnormal/scalar
andSAMEactualpackage/3examples pass,one corebuild/OS,zero otherbuilds.37existing
costs remain unrefreshed; unchanged relevant-path evidence is reused.

[Current byte PNG costs](CURRENT-PNG-BYTE-DECODE-COSTS.md) resolve four more:
RGB8/gray-alpha8/gray8/storedRGBA8 uppers0.721/0.655/0.916/0.748 pass andstop,
3448/2424/1400/4472Bop. Exactpixels/owners/native/quiet all12actuals pass. Stored8
description-only postcheck mismatch is recovered without retiming;original
packet/settings stay fixed. Zero builds or repeatedqualification.33costs remain.

[Indexed/packed current costs](CURRENT-PNG-INDEXED-PACKED-COSTS.md) resolve eight
existingcases withzero builds:palette8/4/2/1 andgray1/2/4 passandstop,uppers
0.886/0.682/0.596/0.586/0.185/0.619/0.598. Adam7gray1 remains1.083/upper1.094,
1440Bop. All12actuals/exactpixels/owners/native/quiet pass;unchanged full/package
proof reused.25costs remain unrefreshed; no workload expansion or retuning.

[Paeth source-layout costs](CURRENT-PNG-PAETH-SOURCE-COSTS.md) resolve four more
existingcases withzero builds:gray8/gray-alpha8/RGB16 passandstop;RGB8 remains
1.085/upper1.104,3448Bop.21existingcosts remain unrefreshed,full268pathobjective.

[RGB8PaethSIMD](WARM-PNG-RGB8-PAETH-SIMD.md) supersedes its source-layoutgap:
19.2%less elapsed,0.880/upper0.882,3448Bop PASS/STOP. Full5107/OS,24kernel+
231publicnormal/scalar,SAMEactualpackage/3examples pass,one corebuild/OS,zero
otherbuilds. Other relevantpaths retain proof;21existingPNGcosts remain.

[Color-key currentcosts](CURRENT-PNG-TRANSPARENCY-COSTS.md) resolve4existingcases:
Gray8tRNS 0.443/upper0.446 pass; Gray16tRNS 1.441/upper1.443 gap; Rgb8tRNS 0.845/upper0.852 pass; Rgb16tRNS 0.700/upper0.705 pass.
All12actuals/pixels/owners/native/quiet pass;zero builds,unchangedproof reused.
17existingPNGcosts remain unrefreshed;full268pathobjective.

[Directgray16keyrows](WARM-PNG-GRAY16-KEY-ROW.md) supersedeits tRNSgap:
76.2%less elapsed,0.341/upper0.342,2456Bop PASS/STOP. Full5129/OS,36kernel+
231publicnormal/scalar,SAMEactualpackage/3examples pass,onecorebuild/OS,zero
otherbuilds.17existingPNGcosts remain;otherrelevantproof reused.

[Palette boundary costs](CURRENT-PNG-PALETTE-BOUNDARY-COSTS.md) resolve four cases:
Palette8, partial alpha 0.902/upper 0.911 pass; Palette8, 16 entries prior1.173/upper1.180 is superseded by prepared lookup upper0.706 pass; Adam7 palette8 1.274/upper 1.283 gap; Adam7 palette4 1.250/upper 1.251 gap.
All 12 actuals, pixels, owners, native and quiet gates pass; zero builds.
13 existing PNG costs remain unrefreshed; full 268-path objective.

[Policy/packed-key costs](CURRENT-PNG-POLICY-PACKED-KEY-COSTS.md) resolve eight:
Pass and stop: Gray2, key0, Gray1, key1, RGBA16, cancelable. Remaining: RGBA16, tight 0.991 / upper 1.002 confidence open; RGBA16, tight + cancelable 1.016 / upper 1.022 gap; RGBA8, tight 1.071 / upper 1.076 gap; RGBA8, tight + cancelable 1.387 / upper 1.395 gap; RGBA8, cancelable 1.140 / upper 1.143 gap.
All 12 actuals, pixels, owners, policies, native and quiet gates pass; zero builds.
Only five PNG stream/composition costs remain unrefreshed; full268pathobjective.

[Stream/composition costs](CURRENT-PNG-STREAM-COMPOSITION-COSTS.md) finish the
five remaining PNG measurements. Seekable16 passes upper0.958/8664Bop;byte
seekable/composed,wordcomposed,async8 remain1.112/1.147/1.124/1.075 gaps. All12
actuals/delivery/pixels/owners/native/quiet pass. One untimedwordinputharnessfix,
zero production/selector/nativebuilds;completedtimings reused;async21controls
andbyteexacttemporaryproofrestoration pass. No unrefreshedPNGcosts;gaps and
openconfidence remain,full268pathobjective unchanged.

[Cancelable row cursors](WARM-PNG-CANCELABLE-ROW-CURSOR.md) improve tight/cancel8
by21.0%,same68224Bop;1.094/upper1.107gap remains. Tight/cancel16 nowpasses
upper0.984/70272Bop. Full5132/OS,122internal+258publicnormal/scalar,SAMEpackage/
3examples pass,onecorebuild/OS/zerootherbuilds. Otherrelevantpaths retainproof.
PNGcostrefreshcomplete;full268pathobjective staysopen.

[Sparse palette stores](WARM-PNG-SPARSE-PALETTE-EXPANSION.md) supersede both
Adam7 palette gaps:54.9%/64.9% less elapsed,uppers0.587/0.440,4976/2608Bop pass.
Gray1 sharedfallback baseline below is superseded by packedgray expansion;
its current upper0.230 passes with1440Bop. Full5153/OS,140internal+258publicnormal/scalar,SAMEpackage/3examples
pass. Onecorebuild/OS,zerootherbuilds;no newbuffer/LUT. Full268objective active.

[Materialized cancelable PNG cursor](PNG-CANCELABLE-MATERIALIZED-CURSOR-REJECTED.md)
fails the predeclared5% screen:4.34%elapsed/4.20%normalized reduction. Removed,
no variants/confirmation;exact qualified source/binaries/package restored,
existing full qualification reused. Accepted costs and268required paths unchanged.

[Shared RGBA predictor histograms](VP8L-SHARED-PREDICTOR-HISTOGRAM-REJECTED.md)
miss the5%screen:1.65%elapsed/0.57%normalized reduction,exact160032Bphotograph
packet. Removed, no variants/confirmation;qualifiedsource/binaries/package and
accepted costs unchanged. Existing full/package proof reused;268paths remain.

[Packed VP8 inverse reconstruction](VP8-PACKED-INVERSE-RECONSTRUCTION-REJECTED.md)
misses5%screen:3.47%elapsed/3.62%normalized reduction,exact17688Bpacket and
809072Bop unchanged. Removed,no variants/confirmation;qualified source/binaries/
package restored,existing full proof reused. All268required paths remain.

[Sparse packed-gray expansion](WARM-PNG-SPARSE-GRAY-EXPANSION.md) closes the named
Gray1Adam7 gap:80.0%less elapsed,0.223/upper0.230,1440Bop passandstop. Exact
pixels/native/source/owners/quiet,5157checks/OS,131internal+258publicnormal/scalar,
SAMEactualpackage/3examples qualify. Onecorebuild/OS,zerootherbuilds;no new
buffer/LUT/capacity. Full268pathobjective remainsactive;unchanged paths reuseproof.

[Prepared small-palette lookup](WARM-PNG-SMALL-PALETTE-LOOKUP.md) closes the named
16-entry Palette8Short gap: 40.5% less elapsed, 0.698/upper0.706, 1512 B/op,
exact pixels and pass/stop. Full5158 checks/OS, 72internal/258public normal/scalar,
SAMEactualpackage/governedconsumer/3examples pass. No new owned capacity, heap
table or cache; one corebuild/OS, zero other measurement builds. Prior PBA1
short-palette cost is superseded; all raw evidence retained. Full268 remains open.

[JPEG column preparation](WARM-JPEG-COLUMN-PLANS.md) accepts7.55%less callerRGB420
elapsed,exact52091/52093B approvedCOM packets. Current1.190/upper1.197 remainsgap;
388222B/op is +14806 measured. GovernedRGBA16 refreshed1.141/upper1.292 short,
819452B/op;sharedrootpeak1704856 unchanged,temporarycleanup/refusal/cancel pass.
Full5163/OS,50internal/66public normal/scalar,SAMEpackage/governedconsumer/3examples
pass. Onecorebuild/OS,zerootherbuilds. Original postcheck failures retained and
existing governed schema/hash checks recovered without measurement reruns.
Affected420/422 costs need applicable evidence before CPU-pass claims;full268
stays open. Stop this preparation primitive; no variants or passing-path retuning.

[Current CMYK fixed-policy cost](CURRENT-CMYK-FIXED-INVERSE-COST.md) resolves
sparse-2D at2.850/upper2.858 confirmedgap,1085508B/op. Original packet/source
blocks/settings/native stayfrozen; independent Q14inverse/Q16device expected
oracles match managednormal/scalar/native exactly. Earlier setup/generator
failures had0actuals; one completed12 confirmation, allraw/guards retained.
No productionchange/full/package repeats;reuse5163checks/OS/SAMEpackage.
Only derived expectedpixels/hash corrected; originals archived, proof overlays
restoredbyteexact before accepted currentoracles installed. Prioritize this
largest material gap; full268 scope and rejected floating axis families unchanged.

[SIMD JPEG dequantization](WARM-JPEG-DEQUANTIZATION.md) accepts13.9%less
sparse-CMYK elapsed with exact pixels:2.453/upper2.462,1085513B/op(+5 measured,
no new owned storage). Affected baseline420 independently current1.239/upper1.245,
757319B/op; callerCMYK3.032/upper3.051,1634905B/op is now the largest confirmed
gap. These are existing named controls, not new coverage. Full5166/OS,
27internal/98publicnormal-scalar,SAMEpackage/governedconsumer/3examples pass.
Onecorebuild/OS,zerootherbuilds;fresh3pair then onecandidate12 and two companion12s.
Old JPEG rendering costs need applicable current evidence before CPU-pass claims;
metadata-only inspection/ValidatePixels skipinverse, other unchanged paths reuse
valid proof. Stop the SIMD preparation primitive; full268-path objective open.

## Finite checklist

The [immutable inventory](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/benchmark-pre-cleanup.md)
names 257 controls. Remove four retired boundaries and add two distinct required
algorithms: **255 existing required cases**. Twelve named controls below cover
the newly added read boundaries: 267 cases. One newly required predictive PNG
algorithm below brings the finite inventory to **268 total**, without a
shape/depth/settings grid.

Retired: PNG resident ImageBudget quota, JPEG420 ImageBudget quota, VP8 caller work
budget32M and optional JPEG CompactColor. Their APIs/encoders no longer exist.
Caller-root controls retain actual shared-admission coverage.

Added, using the existing 513×259 diagram and frozen settings:

- `DiagramLossyTrellis8`, Q75/effort6: coefficient dynamic-programming trellis per
  luma candidate, absent from method4. Exact 6,818-byte packet, SHA256
  `3c98407656841ff97b185e241e76f750f036c972cfe2a0b704ae2eb35cf0adee`.
- `DiagramLossyDistortion8`, Q75/effort2: original-prediction-error mode selection
  before selected reconstruction, distinct from RDO/trellis. Exact 7,330-byte packet,
  SHA256 `325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b`.
- `EncodePredictable16`: default Optimal, original 3000² RGBA16 gradient after
  horizontal flip, R=16x/G=16y/B=8(x+y)/A=65535. Bounded content admission and
  per-row byte prediction are distinct from the existing None-filter encoder
  controls. Functional size <2 MiB and every sample exact are qualified;
  the current complete-operation comparison independently passes at0.941× /
  upper0.979 after bounded word matching,3,402,572B/op with four-operation
  batches. Stop this control unchanged. Native
  level6/pred=mixed is frozen explicitly;
  these packets have a size/pixel gate, not a byte-identity target. No extra grid.

Remaining inventory includes named inspection/Identify stream and resident
boundaries; codec policies, normalization, caller gray/RGB/CMYK and CMYK/YCCK/411/410;
sync/async and retained-root ownership; exact geometry and each required resize
algorithm; logical/OpenType/bidi/Arabic text, fitting and drawing/compositing.
Each existing named case remains required until valid passing evidence applies.
Add a case only when evidence identifies a distinct required algorithm or public
boundary omitted from this inventory.

The new APIs are functionally qualified; current CPU evidence is summarized below.
The following finite extension preserves the original 255 cases. Use existing
original recipes and freeze packet/worker identities before measurement. No new
quality/filter/grid exploration. Async controls use fresh nonseekable input;
other probing uses fresh seekable input and validation uses a borrowed encoded span.

| New control | Existing input | Distinct required work/boundary |
| --- | --- | --- |
| ProbeDimensionsPng | OriginalExif PNG8, 513×259 | IHDR prefix acquisition, without raster/tail validation |
| ProbeDimensionsJpeg | OriginalExif baseline420, 513×259 | SOF acquisition before compressed scans |
| ProbeDimensionsWebp | OriginalExif VP8L alpha, 513×259 | RIFF/VP8 dimension header acquisition |
| ProbeDimensionsJpegAsync | Same JPEG | Actual async prefix acquisition and ownership |
| ValidatePng8 | OriginalExif PNG8, 513×259 | Inflated rows/filter/checksum without RGBA |
| ValidatePng16 | OriginalExif PNG16, 513×259 | Required 16-bit row/workspace boundary |
| ValidateIndexedAdam7 | AuthoredPng c3-d4-adam7-all-filters, 19×17 | Seven passes and palette-index validity |
| ValidateJpegBaseline420 | OriginalExif baseline420, 513×259 | Huffman/coefficient validation without inverse/render planes |
| ValidateJpegProgressive420 | OriginalExif progressive420, 513×259 | Distinct spectral/refinement/progression checks |
| ValidateVp8lAlpha | OriginalExif VP8L alpha, 513×259 | Required ARGB history/inverse transforms without RGBA copy |
| ValidateVp8Alpha | OriginalExif lossy alpha, 513×259 | VP8 planes plus compressed-ALPH coding |
| ValidatePng16Async | Same PNG16 | Complete asynchronous acquisition, retained input capacity and cleanup |

Validation is optional for replacement readiness; its supported public boundaries
still need explicit CPU evidence before full performance completion. Native operations
must return equivalent requested metadata or validate complete coding, with matching
input delivery and ownership. Do not substitute a normalization/output workload when
the operation does not require it.

[Read-boundary evidence](WARM-IMAGE-READ.md) screens all twelve. Baseline420
validation independently passes at0.917× / upper0.935; stop it unchanged.
Progressive420 confirms1.053× / upper1.063. Other non-PNG validation screens remain
1.35–1.45× gaps. Both guards, exact descriptors, independent output/source lifetime
and prefix/coding-scope checks pass. These controls do not refresh default decoding.

[Dimension workspace](WARM-PROBE-WORKSPACE.md) lowers all four probes to
744B/op instead of8680; dimension-only admission is512B instead of8448. Other
fact selections/header scans retain the larger workspace. Both OS full/scalar
and SAME actual package/three examples pass. Validation and unrelated paths are
unchanged, including the independently passing baseline420 validator.

[Completed header acquisition](WARM-PROBE-COMPLETED-READS.md) supersedes probe CPU
costs again: JPEG sync/async independently1.988×/2.119×,46.5%/50.6% less elapsed
than the preceding independent build; PNG/WebP companions1.263×/1.289× short.
All retain744B/op/512B admission. Full both-OS5040 checks,98 focused normal/scalar
and SAME actual package/three examples pass. Probe/InspectHeaders timings change;
validation/Identify/Decode/encoding/editing algorithms and applicable proofs do not.

[Previous one-pass PNG validation](WARM-PNG-VALIDATION.md) independently closed the PNG16
span and async cases:0.990×/upper0.99966 and0.833×/upper0.847. Span elapsed fell
42.4%; allocation240B/op, async8544B/op. These costs are superseded below;
its first upper1.00108 is retained alongside the bounded repeat. PNG8/indexed
companions remain1.355×/1.345× short gaps. Both OS5041 full checks,137 internal/
88 public scalar and SAME actual package/three examples pass. Caller-budget
allocation increases while admitted peaks decrease; separate storage records
are current. SIMD classifier trials have no gain and are removed/closed.

[Current read reconciliation](WARM-READ-RECONCILIATION.md) refreshes six existing
boundaries without production changes. Complete PNG8 headers screen1.365×,
8528B/op against the labeled original FFmpeg AVIO/header adapter. All nine typed
source fields and21 frozen RGBA8 controls agree normal/scalar. VP8+alpha Stream
decode screens1.291×,1966583B/op with exact pixels. Tight PNG8 ordinary/live-token
decode screens1.370×/1.387×,68224B/op; PNG16 screens1.041×/1.029×,70272B/op.
All remained gaps; no independent timing confirmation or speedup claim. The
four earlier PNG decode costs are superseded by the reconciliation below.
Current production qualification/package proof is reused. These six controls do not
refresh every Identify/decoder boundary affected by read-policy changes.

## Current evidence

[Shared PNG Deflate cursor](WARM-PNG-STREAMING-CURSOR.md) independently reduces
PNG8 span validation from 1.862 ms to 1.437 ms: 22.85% less filtered elapsed and
22.87% using all actuals, with unchanged 240 B/op. The 1.042× / upper 1.052 gap
remains. Both PNG16 validation cases are refreshed on the accepted producer:
span upper 0.974 and async upper 0.817, at 240/8,544 B/op. Stop these passing
cases unchanged. Complete coding, all nine source fields, frozen input bytes,
ownership and quiet/native guards pass. Both platforms pass 5,057 full checks,
focused normal/scalar and 212 scalar public PNG checks, plus the same actual
Release package and three examples. No storage/admission boundary is added.

The shared cursor changes noncancellable streaming PNG validation/fused RGBA
decode and full materialization; the cancellable token decoder stays unchanged
but gains a dispatch check. Other PNG decoder CPU evidence is prior-production
until reconciled; the four tight companions are refreshed below. Existing functional
proof remains exact. Counting-only indexed/interlaced validation, encoders,
inspection, other codecs, editing/text and applicable confirmations are unchanged.
The [preceding PNG8 confirmation/profile](current-png8-validation-evidence.json)
retains the diagnostic lead and immutable pre-change producer. No new input or
settings grid, repeated profile or harness/native/selector build was needed.

The [current PNG/VP8 reconciliation](current-png-vp8-reconciliation-evidence.json)
refreshes six existing controls on the same qualified PSC1 producer. Tight PNG8
ordinary/live-token screens are 1.090/1.405×, 68,224 B/op. Tight PNG16 independently
confirms 1.010× / upper 1.015 and live-token 1.040× / upper 1.053, 70,272 B/op.
Their frozen memory ceilings and one-byte-below refusal remain unchanged. The
near-parity outcomes are confirmed gaps; no optimization or gain claim follows.

Opaque RGBA16 VP8 independently confirms 1.412× / upper 1.417, 22.026 ms and
1,201,552 B/op; diagram method4 confirms 1.368× / upper 1.374, 7.938 ms and
741,137 B/op. Complete 72,380/7,100-byte packets and alpha remain exact. All
twelve actuals/filtered results, native brackets and idle-v2 guards pass. All six
VP8 encoding rows now have independent confirmations. This resolves the two wide
screens without new inputs, production/harness/native/selector builds or repeated
full/package qualification. Existing unaffected proofs remain applicable.
The raw archive retains one excluded selector-mismatch attempt and the valid
replacement; it is mirrored and member-verified. The full 267-path goal remains.

The [JPEG AC publication trial](JPEG-BASELINE-AC-PUBLICATION-REJECTED.md) missed
its 5% screen gate at 1.49%. Qualified code/producers were restored, actual package
unchanged; no variants, independent confirmation or full/package repetition.

[Paired JPEG inverse columns](WARM-JPEG-PAIRED-COLUMNS.md) independently reduce
the existing JPEG420 decode by 2.51% (2.44% all actuals), 1.331 ms to 1.298 ms,
with exact pixels and unchanged 757,319 B/op. The 1.285× / upper 1.304 gap remains.
Both interval forms separate; all actuals, native brackets and idle-v2 guards are
retained. Both OS pass 5,056 full checks, affected scalar tests and the SAME actual
package/three examples. Other AVX2 JPEG inverse-render costs become prior-production
until reconciled. Validation skips inverse rendering; its passing evidence remains
applicable. Other codecs, encoding, editing and text paths remain unchanged.

[Current VP8 alpha confirmation](current-vp8-alpha8-encoding-evidence.json)
resolves the existing wide screen at 1.296× / upper 1.306, 69.341 ms and
21,353,208 B/op, with the complete 98,590-byte packet and alpha exact. All twelve
actual/filtered samples, native brackets and idle-v2 guards pass. The immutable
VPS1 producer, cached selector/worker and package qualification are reused.
This is a cost confirmation, without a speedup or regression claim.

[Current JPEG420 attribution](current-jpeg420-decode-attribution-evidence.json)
uses the same qualified build and cached host: 5,630 samples after warmup,
inverse AVX2 32.7% and plain-baseline AC entropy 29.8%. No timing/build/package
repeat. The older profile predates accepted inverse/chroma/color changes. Paired
columns were accepted above; the later AC publication screen was rejected.
These profiles are retained diagnostics rather than current component costs.

[Current VP8 photograph confirmation](current-vp8-photograph-encoding-evidence.json)
resolves the wide screen at 1.467× / upper 1.501, 14.261 ms and 809,072 B/op;
the complete 17,688-byte packet is exact. All 12 actual/filtered samples, native
brackets and idle-v2 guards pass. The same VPS1 producer, IR3 harness, cached
selectors and package proof are reused, with no builds or repeated qualification.
This confirms a cost gap, without a speedup or regression claim. One six-second
profile has 5,925 samples after warmup: Luma4 self 17.7%, reconstruction AddVector
7.4%, coefficient writing 6.9%, quantization 11.1%. The cached profile host is
reused. No inputs, settings or performance boundaries were added. Map the existing
Luma4 samples before choosing a substantial primitive; closed variants stay closed.

The [VP8L group-dispatch screen](VP8L-ENTROPY-GROUP-DISPATCH-REJECTED.md) missed
its 5% gate at 4.71%; the qualified implementation was restored without variants
or repeated full/package qualification. Its authored tile/row/copy regression
remains. Production is unchanged; applicable independent confirmations remain valid.

[The current summary](../docs/benchmark.md) retains 14 unchanged 0.2.0 controls,
refreshes the VP8 Stream decode cost and eighteen read comparisons, and
refreshes three compressed PNG controls on the latest qualified 0.2.1 producer,
retaining the unchanged independent stored-PNG path.
[Bulk stored publication](WARM-PNG-BULK-STORED.md) independently measures PNG16
NoCompression at 1.052× / upper1.067, exact 1,066,552 B and 1,741,709 B/op.
Short screens are not independent confirmations; this representative scope does
not complete the full 267-case goal. [Constant-time distance mapping](WARM-PNG-DISTANCE-SYMBOL.md)
independently measures PNG Fastest at 1.471× / upper1.504; all twelve actual
samples remain. The [residual-summary trial](VP8-RESIDUAL-SUMMARY-DEFERRED.md)
remains deferred/removed; its effort2 baseline costs are superseded below.
The [zero-candidate trellis](WARM-VP8-ZERO-CANDIDATE-TRELLIS.md) and
[effort2 header-bound](WARM-VP8-EFFORT2-HEADER-BOUND.md) primitives remain accepted;
their earlier costs are historical after borrowed cursors below. The earlier
[state-collapse trial](VP8-TRELLIS-STATE-REDUCTION-REJECTED.md) remains rejected.
[The population-window family](VP8L-WINDOW-POPULATIONS-DEFERRED.md) stays closed:
its short gain does not survive confirmation; occupancy/pair reuse is insufficient.
[Scored-plane histograms](WARM-VP8L-SCORED-PLANE-HISTOGRAMS.md) and
[color-trial reuse](WARM-VP8L-COLOR-TRIAL-REUSE.md) remain accepted primitives;
their affected costs are historical after
[shared-history predictor pricing](WARM-VP8L-PREPARED-PREDICTION-COST.md).
RGBA8 independently measures161.623ms versus a fresh174.600ms baseline,
**7.4% less elapsed**,1.332×/upper1.360. Allocation rises29688704→29696928B/op
for one8KiB reusable analysis buffer plus overhead; exact183812B packets remain.
Current RGBA16/async/photograph screens are1.338/1.351/1.322× with29696928/
29697312/20417624B/op and exact183812/183812/160032B packets. They are current
costs, not independent gain/parity confirmations. BothOS5052 full checks,
15internal/27public scalar and SAME actual package/three examples pass.
Diagram4 skips prediction; compressed VP8 alpha uses the unchanged green-only
pricing path. VP8 trellis1.519× and photograph1.479× remain the largest displayed
encoding point gaps; PNGFastest1.471× follows. Prefer substantial absolute-cost
VP8L opportunities where evidenced; do not reopen confirmed passing cases.
[Horizontal JPEG area filtering](WARM-JPEG-HORIZONTAL-AREA-FILTER.md) independently
reduces caller-RGB420 elapsed by16.1% against a fresh baseline, to2.030ms and
1.283×/upper1.294. Allocation373416B/op and exact packet boundary remain.
Governed RGBA16 screens2.252ms/1.224×,804644B/op with unchanged1704856B root peak.
BothOS5049 full checks,4 internal/293 public scalar and SAME actual package/three
examples pass. Stop this primitive; both named CPU gaps remain open. The bounded
VP8 quantization-to-pricing audit found only a32-byte reload/absolute-value reuse,
without a substantial candidate; no production edit or timing followed.
[Borrowed VP8 cursors](WARM-VP8-BORROWED-CURSOR.md) remain accepted. Their
method0/1/2 effort2 evidence remains current; affected RDO costs are superseded by
[exact texture lower bounds](WARM-VP8-TEXTURE-DISTORTION-BOUND.md). Photograph
independently measures14.373ms versus a fresh15.013ms baseline,**4.3% lower**,
1.479×/upper1.483,809072B/op unchanged and exact17688B. Four current screens:
diagram4 1.405×,trellis1.519×,alpha8 1.307×,opaque16 1.331×. All retain exact
packets and quiet/native brackets. Alpha allocation21353208B/op also reconciles
the prior predictor owner's16B overhead increase; other allocations are unchanged.
BothOS5052 full checks,21internal/27public scalar and SAME actual package/three
examples pass. After existing rate pruning,reconstruction and SSD,the nonnegative
texture term cannot save a losing candidate; skip only those spectral transforms.
No new storage,owner,quota or cancellation change. Stop this primitive.
[Uniform-boundary trellis reuse](VP8-UNIFORM-TRELLIS-DEFERRED.md) has no material
independently confirmed gain and is removed; the current baseline stays valid.
Full samples, allocations, packet sizes and boundaries are in the
[immutable release evidence](release-0.2.0-benchmark-evidence.json) and
[current PNG evidence](warm-png-bulk-stored-evidence.json).
The [distance-symbol evidence](warm-png-distance-symbol-evidence.json) replaces
compressed-PNG costs; stored level0 never calls that mapping.

Existing independent confirmations remain usable where their production path,
options, inputs, worker and operation boundary are unchanged. In particular,
default-workspace bicubic RGBA8 513×259→32×16 independently passed at
0.927×/upper0.934 in [the recorded confirmation](WARM-CURRENT-BICUBIC-CROSS-STREAMING.md).
Stop this named path; do not infer passing tight-workspace or other geometry.
The shared-Adler producer is qualified on Windows/Linux with 4,959 checks per OS,
affected scalar coverage and the same actual package/three consumer examples;
[the report](WARM-PNG-SHARED-ADLER.md) retains that source proof.

[Rolling dictionary hash reuse](PNG-ROLLING-HASH-REJECTED.md) is rejected:
its exact count replay does not translate into a material complete-operation
screen gain (0.7%,below5% gate,+16B/op). Production/source/binaries and the actual
package are restored; no confirmation/full/package repeat. Prior applicable
PNG evidence remains valid. Keep this family closed without a distinct lead.

[Resident PNG Identify](CURRENT-PNG-RESIDENT-IDENTIFY.md) now has current9field
boundary evidence:16.133µs versus13.073µs,1.234×/upper1.312,96B/op. All21existing
same-packet controls pass normal/scalar; source color/alpha/components are exact.
The source/native projection supersedes the earlier six-field comparison; this
is a short gap,so no confirmation or optimization follows. Production and actual
package remain unchanged; full/package proofs are reused. Other named resident/
stream Identify boundaries still need typed-fact reconciliation/current costs.

[Resident JPEG/WebP Identify](CURRENT-JPEG-WEBP-RESIDENT-IDENTIFY.md) independently
passes the existing two controls with9source facts:JPEG0.420×/upper0.426 and
WebP0.00436×/upper0.00483,96B/op each. All21controls percodec pass normal/scalar;
complete projection/parser buffering and fresh ownership remain timed. These
replace earlier6field timing proof; stop both passing cases. Production/DLL/
package/full proofs remain unchanged. Stream/async is reconciled below.

[Complete-stream PNG Identify](CURRENT-PNG-STREAM-IDENTIFY.md) now checks9facts
with current delivery/EOF/ownership:seekable screens1.425×/upper1.535,192B/op;
nonseekable async independently1.238×/upper1.271,8400B/op. Its uncertain wide
short is retained and resolved by one12actual observation. All21controls per
boundary pass managed normal/scalar; async also native normal/scalar/ReadAsync-
only gates. No production/full/package repeat. Both remain gaps; no optimization
follows. Resident proofs remain applicable; the three named default decoders are reconciled below.

[Current default decoders](CURRENT-DEFAULT-DECODERS.md) reconcile three existing
boundaries on the same immutable0.2.1core: JPEG420 screens6.018×/upper20.175,
757166B/op; VP8L resident1.425×/upper3.692,1216162B/op; VP8L fresh seekable
stream1.304×/upper1.578,1216190B/op. Every managed/native sample is exact,
including normal/scalar and lossless hiddenRGB. Frozen packets/reference settings
are unchanged; JPEG's old pre-parity managed hash is refreshed after four-way
exact validation. All3actuals/native brackets/idle-v2 are retained; these clear
screen gaps need no extra confirmation. No production/harness/worker/selector
build or full/package repeat. The new materialJPEG gap takes priority for one
bounded current profile. Other sampling/depth/policies/boundaries stay required.

[Exact JPEG inverse SIMD](WARM-JPEG-SIMD-INVERSE.md) independently reduces the
existingJPEG420 decode34.8% (6085.790→3965.366µs),3.936×/upper3.992,still a gap.
Reported allocation757166→757268B/op; explicit256B stack/256B table and owned
capacity unchanged. Generated weights,word bias,Int32 wrap/saturation/rounding
and scalar fallback remain exact. BothOS5054full checks,39internal/293public
scalar and SAME actual package/three examples pass. One corebuild perOS reuses
all harnesses/workers/selectors; no scoring repeats. AllJPEG render/caller-sample
costs are affected; only this named cost is refreshed. Pixel validation skips
inverse/render,keeping its baseline420 pass; Identify/encoding/other codecs and
editing/text retain applicable unchanged proof. Next:one exact quarter-phase
half-resolution chroma-expansion primitive,no inverse variants or workload grid.

[Integer quarter-phase JPEG chroma](WARM-JPEG-QUARTER-CHROMA.md) independently
reduces the sameJPEG420 decode47.2% (3965.366→2095.452µs),2.079×/upper2.241,still
open. Reported757268→757298B/op,unchanged64B tiles/owned capacity. Centered1/3
weights and shared clamped indices keep exact normal/scalar pixels. BothOS5054
full checks and SAME actual package/three examples pass;293public scalar checks
perOS and unchanged inverse proof are reused. No baseline retiming/harness/native/
selector builds. A current6second/5780sample diagnostic,zero hostbuilds,places
chroma22.4%,ACentropy20.2%,inverse kernel19.6% plusentry3.4%,color19.3%. Next:one
SIMD chroma tile with integer edge/tail/single-pixel fallback. Only420/422YCbCr
RGB/RGBA costs change; luma-only/444/411/410/directRGB/CMYK/YCCK and unrelated
paths retain applicable unchanged proofs. No extra image/control or profile repeat.

[SIMD JPEG chroma](WARM-JPEG-SIMD-CHROMA.md) independently reduces the sameJPEG420
case16.7% (2095.452→1746.224µs),1.732×/upper1.862,still open.757298B/op and64B
scratch unchanged; exact normal/scalar pixels. BothOS5054full checks,SAME actual
package/three examples,293public scalar and15existing AVX2-disabled pixel/caller
controls pass. Existing scalar/baseline proof reused; zero baseline retiming or
harness/native/selector builds. Wide short upper30.849 retained,one independent
confirmation; no scoring repeats or new image/grid. Vector-eligible420/422costs
change; integer edges/tails/single-pixel arithmetic and unrelated paths retain
applicable proof. Current selected cost refreshed; other affected controls need
current proof before pass claims. Next:eight-pixel color conversion,using the
retained JCP2 profile; no unchanged profile or chroma/inverse variants repeated.

[SIMD JPEG color](WARM-JPEG-SIMD-COLOR.md) independently reduces the sameJPEG420
case18.4% (1746.224→1424.367µs),1.396×/upper1.518,still open.757319B/op (+21),no
new owned storage. SameQ13 coefficients/rounding/clipping,exact normal/scalar pixels,
RGB/RGBA packing/padding and fallback. BothOS5054full,SAME package/three examples,
8color normal/scalar,293public JPEG normal/scalar and15AVX2-disabled controls pass.
Baseline/focused proof reused;zero baseline retiming or harness/native/selector
builds. Bulk byte-plane color costs change; selectedJPEG420 refreshed,other affected
controls need current proof before pass claims. The subsequent VP8 input round
below supersedes earlier VP8 encoding costs. A bounded pre-input6second VP8profile
(5912samples,exact6818B packet) places40.7% in Apply;one hostbuild,no production/
native rebuild or gain claim. Instruction attribution and one
[quadratic distortion trial](VP8-QUADRATIC-DISTORTION-REJECTED.md) are complete:
the DP loop accounts for23.0% of all samples, but the trial screen is15.0% slower
with exact packets and unchanged allocation. Qualified source/binaries are restored
on both OS; no confirmation/full/package repeat. Close this arithmetic family.
Next resolve the photograph's attribution after accepted texture pruning before
selecting one distinct substantial primitive. That
[current photograph attribution](warm-vp8-photograph-attribution-evidence.json)
is now retained:5927workload samples,Luma4self17.5%,quantization10.9%,
reconstruction8.0%;exact17688B packet,quiet guards,no builds or timing claim.
This bounded review establishes no distinct substantial candidate. Defer further
photograph variants. A bounded
[PNGFastest count replay](png-fast-match-counts-evidence.json) finds94.1% selected
copies shorter than8bytes,3.33extension comparisons per attempt,exact236828B
packet. No substantial bulk match-extension lead is established; no production
candidate or timing run. Do not reopen closed state-collapse/
uniform/scan/batch families or repeat unchanged profiles.

[SIMD VP8 input preparation](WARM-VP8-SIMD-INPUT.md) independently reduces the
effort 2 diagram's mean by 13.2% using all actual samples (14.3% filtered), to
3.382 ms, 1.345× / upper 1.423; 741,375 B/op and an exact 7,330 B packet.
The earlier independent 5.4% pair overlapped; one final reverse-order pair
separates filtered and unfiltered intervals. All observations and material native
drift remain retained. Five other displayed VP8 controls have wide short bounds,
without gain/regression claims. Both systems pass 5,055 full checks, 14 preparation
normal/scalar controls, 27 scalar public packet controls, the SAME actual package
and three examples. AVX2 alpha/luma preserves Q16 math, tails, padding, portable
fallback, row cancellation and capacity; no owner or storage is added.
Stop this primitive. Resolve material wide companion measurements before choosing
the next opportunity; the full 267-path warm upper ≤1× objective remains.

The [current trellis confirmation](current-vp8-trellis-encoding-evidence.json)
resolves that wide effort 6 screen: 15.160 ms, 1.539× / upper 1.551, 754,344 B/op
and the same exact 6,818 B packet. Twelve actual/ten filtered samples, native
brackets and idle-v2 guards pass. The immutable VPS1 build, cached selector and
worker are reused; no builds, tests or package checks are repeated. This is a
confirmed gap, without a before/after gain or regression claim. The bounded
native strategy review establishes no distinct substantial trellis candidate;
keep closed variants closed and attribute PNGFastest before editing.

The [PNGFastest trace](png-fastest-attribution-evidence.json) now supplies that
attribution: 5,974 workload samples, 47.6% in match searching, 16.7% in bit writing
and 12.1% in dictionary insertion. The same 236,828 B packet, pixels, ownership
and quiet guards pass. One tiny host is built; production/harness/native builds
and package/suite repetitions are unnecessary. This is diagnostic evidence only.
Screen one word-buffered bit-emission primitive, preserving byte/flush/failure
boundaries; keep hash and bulk match-extension trials closed.

The [word bit-buffer screen](PNG-WORD-BIT-BUFFER-REJECTED.md) gives only 3.7% less
elapsed, below its 5% gate, and adds 125 B/op. Exact packets, flush/failure cleanup
and focused normal/scalar controls pass on both systems. It is rejected; qualified
source, matched binaries and linked tests are restored. No confirmation or
repeated full/package qualification follows. Keep this family closed.

The [dictionary-width trial](PNG-DICTIONARY-WIDTH-REJECTED.md) saves 256 KiB of
owned capacity and 19.5% measured allocation, but only 1.3% elapsed in its short
screen, below the 5% CPU gate. Exact bytes, cleanup and high-index comparisons pass
normally/scalar on both systems. It is rejected and restored without confirmation
or repeated full/package checks; retain the allocation evidence for later work.

The [insertion-inlining trial](PNG-INSERTION-INLINING-REJECTED.md) is also flat:
7.039 ms baseline versus 7.038 ms candidate, with identical allocation and packets.
Matched instruction attribution established the call boundary, but its measured
complete-operation result fails the 5% gate. Restore and stop this family;
unchanged qualification is reused. Next resolve the wide VP8L resident-decode
control before selecting a distinct substantial opportunity.

The [VP8L resident confirmation](current-vp8l-resident-decode-evidence.json)
resolves its wide screen at 4.583 ms, 1.333× / upper 1.404, 1,216,162 B/op.
All twelve actual/filtered samples, native brackets and quiet guards pass on
the immutable VPS1 producer, with exact pixels, alpha, hidden RGB and independent
outputs. No builds or qualification checks repeat. One bounded workload-only
trace places 34.7% in ReadImage and 33.1% in Huffman.Read; those are attribution
leads, without phase-time or speedup claims. Reuse its instruction samples before
choosing a distinct substantial primitive; the full 267-path objective remains.

## Evidence invalidation

Regression/pixel/reference evidence and timings are separate:

- SIMD RGBA8 VP8 alpha scanning and opaque luma change preparation costs across
  efforts 0–6 and normalization/stream/async/cancelable callers. Shared Create layout
  also changes; opaque16 is refreshed though its scalar math remains. All six
  displayed VP8 encoding controls are current; other affected inventory cases
  need applicable current costs before a CPU-pass claim. VP8 decoding,
  VP8L/PNG/JPEG/editing/text and applicable unchanged confirmations remain valid.
  No new owned capacity is introduced.

- Fuel/ownership cleanup changed polling, codec framing and nested callers. Earlier
  end-to-end costs do not establish current performance. Bicubic extraction kept
  exact arithmetic but changed code layout; older costs require measurement.
- JPEG inverse/color changes invalidate decoder/caller output costs. JPEG entropy,
  MCU normalization and record replay affect encoding only.
- Horizontal area filtering changes AVX-512BW subsampled JPEG420/422 encoding,
  including caller samples,8/16-bit normalization, stream/async, restart and
  cancelable callers. Caller-RGB420 and governed RGBA16 costs are refreshed;
  other affected inventory paths need applicable current evidence before a CPU
  pass claim. Grayscale/444, scalar/tiny edges, all decoding and other codecs/
  editing/text remain unchanged; packet and functional proofs stay valid.
- VP8 shared prediction changes encoder and decoder callers. Source analysis,
  quantization, reconstruction, interval coding and signed-domain checks affect
  their encoder effort/depth/alpha/stream/async callers, not unrelated codecs.
- VP8L indexed histogram replay affects lossless encoding and compressed VP8 alpha,
  including normalization/async boundaries; opaque VP8 is unaffected.
- Shared Adler reuse affects every PNG encoding policy/color/depth and sync/async
  delivery caller. PNG decoding, other codecs and editing/text are unchanged.
- Bulk stored publication changes level0 and the stored fallback in compressed
  PNG policies. All PNG encoding callers require applicable new costs; four
  displayed controls are refreshed. Unrelated paths and native oracles remain.
- Constant-time distance symbols affect compressed PNG policies, including stream/
  async callers; three displayed controls are refreshed. Stored level0 is unchanged
  and keeps its independent proof. This does not invalidate unrelated code paths.
- Exact color-trial reuse changes lossless color-search encoding, including
  normalization, stream/async and applicable method5/6 prediction families with
  nonzero red/blue. Four displayed callers are refreshed. Diagram4 skips prediction/
  color search and normalized VP8 alpha has RedBlueZero, so their costs remain
  valid. Other affected inventory cases need applicable current timings before a
  CPU-pass claim. All decoders, unrelated codecs/editing and native workers remain.
- SIMD scored-plane histograms affect the same lossless color-search callers;
  four displayed costs are refreshed above. Other affected named method5/6/opaque
  callers need applicable current costs before a pass claim. No owned capacity,
  admission or cancellation boundary changes. Unrelated proofs remain valid.
- Whole-block zero-candidate preflight changes method5/6 luma-trellis encoding;
  the displayed diagram6 cost is independently refreshed. Other affected named
  trellis callers need applicable current costs before a pass claim. Method4/
  effort2 bypass trellis, and all other codecs/decoding/editing/text remain.
- Header lower bounds change distortion-only method0/1/2 small-block encoding.
  The effort2 diagram is independently refreshed. Other affected named effort/
  depth/alpha/stream callers need applicable current evidence before CPU-pass
  claims. Method4/5/6 RDO/trellis, all decoding, other codecs/editing/text and
  applicable unchanged evidence remain valid. No storage/ownership change.
- Borrowed cursors affect all VP8 partition emission, including every effort,
  depth/alpha/stream/async/cancelable encoder caller. Six displayed controls are
  refreshed; other affected named cases need applicable current costs before a
  CPU-pass claim. VP8L compressed alpha work itself, all decoding, other codecs/
  editing/text and applicable unchanged evidence remain valid. Physical owned
  capacity, logical growth admission and output ownership are unchanged.
- Release documentation and package-document changes do not invalidate unchanged
  production behavior or applicable performance proofs. The release snapshot uses
  the exact packaged DLL and refreshes actual producer-bound frame/async proofs.
- Selective probing adds the explicit prefix boundary measured above; full CPU
  parity remains open. Reader/parser changes affect InspectHeaders timings, which
  need applicable current evidence before a pass claim. The existing complete
  seekable PNG header control is now refreshed; it is distinct from prefix
  probing/validation. Identify, pixel codecs, transforms and native workers remain.
- Optional metadata policy changes read-entry validation and header preflight for
  Identify, Decode (including caller JPEG samples), InspectHeaders and Probe.
  Pixel/packet oracles stay valid, but affected complete-operation timing/allocation
  evidence needs current confirmation before a pass claim. The displayed VP8
  stream decode now has a current-cost screen. Encoding/editing contexts, kernels,
  ownership and their applicable confirmations remain unchanged; no whole-library
  CPU refresh or new quality/shape matrix is required.
- Pixel validation shares PNG row reading, JPEG entropy parsing and VP8L ARGB
  decoding with normal decode. Affected decode costs remain uncertain until
  current confirmation; functional/native-pixel proofs are refreshed. New validation
  allocation/admission observations are [scoped separately](PIXEL-VALIDATION-STORAGE.md).
  Current validation CPU evidence is in [read boundaries](WARM-IMAGE-READ.md);
  storage observations themselves contain no timing claims. Encoding/editing/native
  workers stay unchanged.

Frozen inputs/settings/workers, approved native-only adjustments and exact oracles
remain reusable when unchanged. Retired quota cases cannot be failures or passes.
Source/path changes must identify affected older evidence, not invalidate the
whole library by default. Older histories remain in
[the preceding coverage record](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/benchmarks/WARM-COVERAGE.md)
and their individual reports/raw archives.

## Next performance work

The high-priority [predictable PNG regression](PNG-PREDICTABLE-COMPRESSION.md) is
functionally closed: all six opaque geometries are below 574 KB, exact samples,
both-OS full/scalar and same actual package checks pass. Its four displayed
existing encoder costs are current; all remain gaps. Other affected PNG encoder
costs need reconciliation before pass claims. The new predictive algorithm's
[bounded word matching](WARM-PNG-WORD-MATCH.md) halves elapsed against a fresh
independent baseline, preserving allocation,exact573,041-byte packets andallgates.
A four-complete-operation confirmation resolves execution-rate uncertainty at
0.941× / upper0.979; stop predictive PNG unchanged. Two single-operation
confirmations with wider bounds andone invalid/excluded selector run are retained.
BothOS5,070, affected scalar andsameactualpackage pass. SmallestSize8/16 current
screens remain1.330×/1.324×; no gain/regression orpass claim. Other affectedlevel6/9
PNG encoder costs need evidence beforepassclaims. Resume remainingmaterialgaps;
higherJPEGprobe ratios remainopenatsubmicrosecondcosts. Decoder,
inspection, other-codec and editing algorithms retain applicable evidence.

[Bicubic horizontal tap views](WARM-BICUBIC-HORIZONTAL-TAPS.md) are accepted:
Down8 independently improves by 11.5% filtered / 13.1% all-actual elapsed, retaining
exact pixels and unchanged storage. Its 1.339× / upper 1.347 gap stays open;
Up16's DHP1 screen is superseded by shared word publication below. Both platforms pass 5,074 checks
and the same actual package/three examples, including cached 8/16-bit pixels,
admission refusal/recovery and retained-output ownership.

This changes the general horizontal loop selected by nine existing controls:
Down8/16, Up8/16, Cross8/16, Horizontal8/16 and HorizontalUp8. The
[bounded reconciliation](CURRENT-BICUBIC-CACHED-COSTS.md) now supplies current
costs for all nine: Down16 and three horizontal cases are independently confirmed
gaps; Up8/Cross8/Cross16 are clear-gap screens. No builds or repeated full/package
proofs were needed. Horizontal8 preserves its existing six certified rational
alpha halves and exact qualified pixels; no new exception is introduced.
This reconciles changed paths in the finite inventory without a new
shape/depth/filter grid. Mixed8/16 select earlier column
kernels; vertical axes copy decoded rows; tight-workspace, wider-support and
word-horizontal-enlargement paths leave before the changed loop. Other codecs,
filters, text and read boundaries retain applicable unchanged confirmations.

[Shared word publication](WARM-BICUBIC-SHARED-WORD-PUBLICATION.md) removes 43
duplicated normalization/packing lines and reuses the qualified word-pair helper.
Up16 independently falls by 15.1%, retaining exact pixels, at 1.221× / upper1.234;
measured5,016,602B/op (+8), with no new owned buffer. The five affected default
word paths all have current costs: Up16,Down16,Horizontal16,Vertical16,VerticalUp16.
**VerticalUp16 independently passes at0.964× / upper0.973; stop it unchanged.**
Down16 confirms1.044× / upper1.050; horizontal/vertical reduction remain screen
gaps. BothOS5,074,89focusednormal/scalar andSAMEactualpackage/3examples pass.
The native-only low-alpha RGB≤2 qualification is retained for VerticalUp16;
managed pixels stay exact. Byte/mixed-word and other algorithms remain unchanged.
These existing inventory paths add no workload grid; the full268 objective remains.

[Byte zero-tap trimming](WARM-BICUBIC-ZERO-TAPS.md) independently reduces
HorizontalUp8 elapsed by16.2% filtered/16.1% all actuals, unchanged1,930,356B/op.
All six changed default-byte paths have current costs: Down8,Up8,Horizontal8,
Vertical8,HorizontalUp8,VerticalUp8. All remain gaps; Horizontal8's independently
confirmed upper1.008 is not a pass. Existing rational-half/native qualifications
remain exact and unchanged. Full5,074perOS,89focusednormal/scalar andSAMEactual
package/threeexamples pass. Word publication, including passing VerticalUp16,
mixed-word/tight/wide kernels and other algorithms retain applicable evidence.
This supersedes earlier byte-publication costs without expanding the inventory.

[Trellis caller invariants](WARM-VP8-TRELLIS-CALLER-INVARIANTS.md) independently
reduce the existing diagram/effort6 control by4.98% filtered/5.14% all actuals:
1.439× / upper1.451,unchanged754,344B/op and exact6,818B packet. Public validation,
cancellation, admission and ownership stay intact; only repeated private checks
of caller-established invariants go. Full5,074perOS,21focusednormal/scalar and
SAMEactualpackage/threeexamples pass. Other method5/6 callers selecting Apply
need applicable current costs before CPU-pass claims. Methods0–4, other codecs,
decoders, editing/text and passing resize cases retain applicable unchanged proof.
No path or parity pass is added; the full268 objective remains open.

The prior [TightCross16 confirmation](CURRENT-BICUBIC-CROSS16-COST.md), now
superseded by output-resident rows, resolved the wide
short-screen confidence:1.570× / upper1.580,1,285.382µs,1,063,410B/op. All twelve
actuals and exact pixels/native bounds/owners/guards pass. No gain/regression
claim against earlier screens. Current immutable TGV1 producer and cached
selector/worker/fixture are reused with zero builds or repeated qualification;
CrossWord/math/dispatch source pins preserve applicable attribution. No new path.

[Ordinary quantization caller invariants](WARM-VP8-QUANTIZATION-CALLER-INVARIANTS.md)
supersedes the earlier six displayed VP8 encode costs. Photograph independently
improves by 6.93% filtered / 6.90% all actuals: 1.433× / upper 1.438, unchanged
809,072 B/op and exact 17,688-byte packet. Five affected companions have current
short screens; all remain gaps, with unchanged allocations and packets. Other
method0–6/depth/alpha/stream/async paths selecting ordinary quantization need
applicable current costs before CPU-pass claims. Full 5,065 perOS, 41 focused
normal/scalar, 27 public scalar and SAME actual package/three examples pass.
Nine unreachable private invalid-input test rows go; all public/live checks,
native hashes and 73,504 arithmetic blocks stay. Other codecs, decoding, editing/
text and passing resize paths retain applicable unchanged evidence. No new path
or parity pass; the full 268-path objective remains open.

Resolve uncertainty before optimization; optimize demonstrated largest gaps and
stop confirmed passing cases. Stored publication and distance mapping are accepted.
The fixed-importance trial is [deferred](VP8-FIXED-IMPORTANCE-DEFERRED.md): an
uncertain 2.2% short difference does not justify more variants. The high-priority
probing/metadata/validation milestones are functionally complete. The bounded
read reconciliation is recorded; its six controls remain CPU gaps. PNG16 span/
async validation pass and stop unchanged. Scored-plane histograms are accepted;
prioritize substantial VP8 photograph/effort2 and caller-RGB420 gaps before
smaller variants or submicrosecond wrappers. Other affected named read/decode
costs need current evidence before a pass claim; no broad readiness claim follows.
Establish a substantial distinct opportunity before editing. Distinguish
shared work from delivery/normalization, reuse unaffected evidence and verify
affected companion boundaries on acceptance.
Do not renew the deferred uniform-trellis, population-window/occupancy/pair-cache, residual-summary
or rejected trellis state-reduction
families without a materially different opportunity.
The [CrossWord horizontal-pair trial](BICUBIC-HORIZONTAL-PAIR-REJECTED.md)
also fails its5% screen gate and is fully restored. Its exact-pixel/ownership
checks pass; no variants or repeated full/package qualification. Keep the
independent TightCross16 confirmation current and close this pairing family.
The distinct [output-resident row-cache trial](BICUBIC-OUTPUT-ROW-CACHE-UNQUALIFIED.md)
has23.723%lower confirmation means but fails its predeclared native-stability
limit (7.688%drift versus3%). It is unqualified, not an accepted gain or parity
pass. Production/exact binaries were restored for that trial; six useful precision/ownership/
cleanup controls remain. No variants or repeated confirmations; unchanged
full/package evidence and accepted costs carry forward.
Closed source-energy, histogram,
word/fragment, prediction/quantizer/trellis and other rejected variants stay closed.
Use immutable builds and narrow jobs; batch full Windows/Linux/scalar/package
checks for accepted production changes. Preserve complete operation boundaries,
exact pixels/packets, shared admission and quiet-VM confidence requirements.
- [CHB1/CT1 coefficient trials](WARM-VP8-COEFFICIENT-TRIALS.md) are closed and
  reverted. Headroom missed its 5% screen gate; retained tokens independently
  saved only 1.21% elapsed while allocation grew from 1,201,552 to 2,331,536 B/op.
  Do not renew either family without materially different evidence.
- Prepared predictor pricing changes generic four-plane lossless prediction,
  including method4/5/6, normalization and stream/async callers that select it.
  Four displayed controls are refreshed; other affected named controls need
  applicable current costs before CPU-pass claims. One8KiB product array is
  admitted with the existing analysis owner and released before row application.
  Green-only alpha pricing, paths skipping prediction, all decoding, other codecs,
  editing/text and their applicable unchanged confirmations remain valid.
- Reconstructed-SSD texture bounds change method3/4/5/6 Luma4 RDO encoding,
  including depth/alpha/stream/async/cancelable callers. Five displayed controls
  are refreshed; other affected named cases need applicable current costs before
  CPU-pass claims. Method0/1/2 is unchanged and retains effort2 evidence. No owned
  capacity or cancellation boundary changes; VP8L/other codecs/decoders/editing/
  text and their applicable unchanged proofs remain valid. The prior predictor
  Analysis owner also gained a reference field on green-only calls; the current
  alpha row reconciles its16B allocation increase,without changing green-only
  arithmetic or admitted capacity.
