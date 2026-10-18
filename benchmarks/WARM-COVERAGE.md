# Material warm coverage

Updated 2026-10-18. The **warm single-CPU parity objective remains open**:
63 independent passes, 128 measured mean gaps and one confidence-open case across
192 current rows; 17 prior-production JPEG decode/inspection rows need reconfirmation.
The [three-table summary](../docs/benchmark.md) retains all 209 numeric rows:
122 current independent confirmations, 70 current qualified shorts and 17 prior rows.
Reuse unchanged passing paths;
do not add an image or settings grid.

Shared-budget codec entry points add two required, **unmeasured** representatives:
`SharedBudgetDecode/PngRgba8Resident` reuses `WarmPng/Decode8` to cover complete
decode and retained-budget owner disposal; `SharedBudgetEncode/Jpeg420Rgba8`
reuses `WarmJpeg/Encode420_8` to cover complete output streaming and work settlement.
Reuse the existing inputs, settings, native references and quality gates. These
cover the new common admission/ownership boundaries, not new codec algorithms.
These admission additions leave original successful codec paths and ImageBuffer
disposal unchanged. The later JPEG compatibility changes invalidate only the
JPEG decode/inspection timings described below.
Budgeted entry points have correctness/package qualification, pending separate
quiet-VM elapsed/allocation evidence; existing parity claims do not cover them.

Direct JPEG sample decoding adds two further **unmeasured** representatives:
`JpegSamplesDecode/GrayscaleResident` reuses `WarmJpegDecode/OwnedDecodeGray`, and
`JpegSamplesDecode/Rgb420Resident` reuses `WarmJpegDecode/NativeDecode420`.
Each includes a fresh caller destination, complete decode and independent output
lifetime, with equivalent native gray/RGB sample delivery. They exercise the
distinct single-channel copy and three-channel color-writing boundaries without
an RGBA frame. Packed rows, original inputs/settings and existing JPEG quality
gates are fixed; padded rows and opt-in orientation have correctness coverage.
JPEG sample decoding now also supports CMYK, as described below.

Direct JPEG sample encoding adds two now **screened** representatives:
`JpegSamplesEncode/GrayscaleResident` uses the fixed grayscale projection and the
approved SIMD-enabled libjpeg-turbo reference; `JpegSamplesEncode/Rgb420Resident`
uses the fixed opaque RGB projection and FFmpeg's 4:2:0 reference. Retain existing
quality/size matching, with equivalent raw sample delivery, complete row
normalization, streaming and independent encoded output ownership. The cases
cover single-component samples and packed RGB conversion, without a settings grid.
The four budget/decode boundaries above remain unmeasured. The [two direct sample
encode screens](WARM-JPEG-SAMPLE-ENCODE.md) measure gray1.145x/upper1.363x,
155,007B/op and RGB4202.004x/upper2.216x,297,896B/op. Both are mean gaps, so no long
confirmation follows. Native raw-input adapters use the same pinned libraries
and exact frozen packets; each operation includes necessary normalization/coding
and fresh independent output ownership. Production and H4 qualification are reused.

The [current direct-sample quality check](warm-jpeg-sample-quality-evidence.json)
reproduces both frozen managed packets exactly, using the same original gray/RGB
projections. Gray:58,823/58,910 bytes and PSNR gap0.000033dB; RGB420:74,040/52,093
bytes and gap0.106110dB. Both pass the existing0.25dB gate, decoder agreement,
source preservation and independent-output checks. This proves reusable quality
and size anchors. The new raw-input screens supply the distinct time/allocation
evidence; existing RGBA operation timings were not substituted for these paths.

JPEG compatibility changes shared frame/header parsing (one/three/four components),
scan setup (four component slots), sampling validation (horizontal ratios up to
four), Adobe marker handling and final decode dispatch. Therefore **all 17 existing
JPEG decode/inspection rows** lose current-code timing validity, including the
fresh-Stream restart case and resident Identify. Their exact old numbers remain
historical. Entropy/IDCT kernels are unchanged, but that alone does not prove the
complete operation unchanged. JPEG encode, PNG/WebP and editing evidence is reused.
Resolve these uncertainties before assuming optimization is needed.

Five further **unmeasured** representatives cover the newly required algorithms:

- `JpegDecode/CmykAdobeResident`: four component planes, Adobe inversion and
  complete owned RGBA conversion from an original CMYK projection.
- `JpegDecode/Ycck420Resident`: subsampled color reconstruction, explicit matrix
  transform and a full-resolution black plane, using the same original input.
- `JpegSamplesDecode/CmykResident`: complete caller-owned canonical CMYK output;
  establish an equivalent native raw-component delivery boundary before timing.
  Native RGB output alone does not cover this operation.
- `JpegDecode/411Resident`: quarter-width horizontal chroma reconstruction on the
  existing RGB projection; confirm native sample-grid equivalence before timing.
- `JpegDecode/410Resident`: quarter-width, half-height chroma (JPEG factors 4×2,
  not video pixel format 4×4); confirm native sample-grid equivalence before timing.

Baseline/progressive analytical and live FFmpeg compatibility checks cover these
layouts; they are not performance confirmations. Reuse the existing source
projections and fixed settings. Including the async boundaries below, the
bounded checklist is **224 named cases**: 209 retained numeric rows plus fifteen
unmeasured new operations. Do not add a
shape, depth or quality grid. Add cases only for a distinct required path omitted
from this list, with its concrete reason.

Async input adds exactly two **unmeasured** common front-end boundaries:
`AsyncStreamIdentify/PngRgba8FreshNonSeekable` and
`AsyncStreamDecode/PngRgba8FreshNonSeekable`. Both reuse the existing PNG8 input
and full structural/pixel gates, adding actual async reads through EOF, fresh
private input acquisition/growth/release and independent descriptor/pixel owners.
They cover the shared async acquisition path and its two different complete
public operations. Shared-budget awaits have correctness coverage; no new
codec/depth/stream scheduling grid is added. Resolve equivalent native input
delivery before timing. The known finite checklist is now **224 cases**: 209
numeric rows plus fifteen unmeasured operations. All existing evidence keeps its
current status: original synchronous method bodies, field layouts, kernels and
ownership paths are unchanged; the seventeen prior JPEG rows still need reconfirmation.

Async output adds four **unmeasured** representatives, all on existing inputs:

- `AsyncStreamEncode/PngRgba8FreshNonSeekable`: batched chunk staging, complete
  encode, asynchronous delivery and independent caller-owned output.
- `AsyncStreamEncode/JpegRgba8FreshNonSeekable`: the distinct stock-memory adaptive
  admission probe and bounded contiguous staging for the existing RGBA8 anchor.
- `AsyncStreamEncode/JpegRgb420Samples`: direct borrowed RGB memory and the separate
  sample/adaptive admission boundary, reusing the fixed RGB420 projection.
- `AsyncStreamEncode/WebpLosslessRgba8FreshNonSeekable`: byte-at-a-time bit-writer
  emission into staging, distinct from PNG's batched writes, with exact hidden RGB.

These cover distinct new staging/admission/delivery paths, without a codec,
sampling, depth or scheduling grid. Resolve equivalent native destination
acquisition/copies and output lifetime before timing. Shared-await correctness
is separately qualified. The checklist is now **224 cases**: 209 numeric rows
and fifteen unmeasured operations. Existing numeric statuses are unchanged;
original synchronous method bodies, state layouts and codec kernels are untouched.

Latest [wider RGBA8 horizontal reversal](WARM-HORIZONTAL-FLIP512.md) improves
single-output flip **22.0%**, passing parity at **0.950× / upper 0.965×**. Two live
outputs improve **27.9%** but remain a qualified **1.053×** gap. Both retain
**128 B/image**, exact pixels, hidden RGB and independent owners. The RGBA16
passing kernel and original fallbacks remain unchanged; no new allocation or
resource/work charge. At that round, other 205 comparisons/all 63 prior passing
proofs remained valid; the JPEG rows above now require reconfirmation.
All 3,739 Release tests/OS, 67 affected public hardware-off checks/OS and the same
actual private package consumer/all three examples pass on Windows/Linux.
The newly passing single-owner path stops; no width/unroll/inlining grid follows.

Earlier [partial PNG transparency](WARM-PNG-PARTIAL-ALPHA.md) retains its 33.3%
gain and passing proof; fixed JPEG fields, packed bicubic reduction and PNG/JPEG
reconstruction retain their measured gains. Those executed paths remain unchanged.

The finite checklist's **previous <=2x phase was completed for released0.1.0**.
Its original independent evidence and unchanged-path proofs remain historical;
subsequent production changes identify exactly which current rows they supersede.
Only independent upper<=1 paths are stopped for the present objective. The coverage
items below name required algorithms and public boundaries; links marked closed
refer to that previous phase. Current results and allocations are in the summary.

Scope is the existing two original513x259 RGBA8/16 rasters, documented projections
and the named fixed selectors in the linked evidence/renderer. Required still
PNG/JPEG/WebP codecs,8/16-bit edits, orientation/crop/nearest/bilinear/bicubic,
resident/fresh-Stream and existing cancellation/limits/owner boundaries are
represented. Cases were chosen for distinct material algorithms or public
boundaries, not a Cartesian grid of settings. The61 bicubic selectors were already
established before this audit and were reconciled, not expanded.

| Item | Named fixed scope and reason | Current evidence / state |
| --- | --- | --- |
| B01 | Bilinear Down8/16,Up8/16: shared resize dispatch and byte/word alpha-aware filtering | [Previous phase closed](WARM-CURRENT-BILINEAR.md); [Down8](WARM-BILINEAR-BYTE-ROW-CACHE.md), [Down16](WARM-BILINEAR-WORD-CACHE.md) and [Up8](WARM-BILINEAR-BYTE-FACTORIZATION.md) improved, still <=1x gaps |
| B02 | JPEG adaptive Encode8/16,Encode422_8/16: changed AC planner at444/422 and both input depths | [All four closed](WARM-CURRENT-JPEG-ADAPTIVE.md) |
| B03 | JPEG Decode8,NativeDecode8,NativeDecode420/422,OwnedDecodeGray,NativeDecode420Restart,DirectRgb422/420: changed ordinary AC loop plus distinct component renderers | [All eight closed](WARM-CURRENT-JPEG-DECODE.md) |
| B04 | JPEG ProgressiveGray/420/422 plus ordinary/live-token444: masked refinement and its guarded fused readers | [Four ordinary proofs reused](WARM-JPEG-REFINEMENT-FUSED.md); [live-token444 improved again](WARM-JPEG-REFINEMENT-LOOKAHEAD.md), still a <=1x gap |
| B05 | The25 named PNG decode selectors in the linked report: byte/word predictors, expansion and input-pool changes | [All25 closed](WARM-CURRENT-PNG-DECODE.md) |
| B06 | PNG DecodeAdam7_16,DecodeGray1Adam7: interlaced placement on changed shared framing; later palette4/8 cases reuse their own proofs | [Closed](WARM-CURRENT-PNG-ADAM7.md) |
| B07 | Png8/16,PngCropFit8/16: complete fresh input, orientation/crop/fit and intermediate ownership | [Closed](WARM-CURRENT-PNG-STREAM.md), with word fusion proof reused |
| B08 | PNG Encode8/16: changed RGBA packing, exact samples and fixed IDAT-size policy | [Both closed](WARM-CURRENT-PNG-ENCODE.md) |
| B09 | VP8 Encode8/16 with alpha: changed output writer and word normalization | [Both closed](WARM-CURRENT-VP8-ENCODE.md) |
| B10 | VP8L Lossless8 literal/run input: changed entropy/run path | [Closed](WARM-CURRENT-VP8L-LITERAL.md) |
| B11 | Clone,crop,normal orientation,horizontal/vertical flip,half turn,CW/CCW,transpose,transverse at both depths: shared output-pool acquisition | [All20 closed](WARM-CURRENT-EXACT-EDITS.md); retained-two flip unchanged |
| B12 | The61 named bicubic selectors: changed dispatch/helper/guard paths, default/tight scratch, byte/word and established support widths | [Previous phase closed](WARM-CURRENT-BICUBIC.md); [word](WARM-BICUBIC-PAIRED-COLUMNS.md) and [byte](WARM-BICUBIC-BYTE-PAIRED.md) paired columns plus [byte strips](WARM-BICUBIC-BYTE-STRIPS.md) and [byte](WARM-BICUBIC-WEIGHT-BROADCAST.md) and [word](WARM-BICUBIC-WORD-BROADCAST.md) packed weights improved, still <=1x gaps |
| B13 | Existing PNG stored/Paeth/predictor,RGB/gray/gray-alpha8/16,packed gray1/2/4,palette1/2/4/8,tRNS,Adam7 and frozen compression-mode rows; four named exact-limit RGBA8/16 ordinary/live-token boundaries | [Up8/16](WARM-PNG-UP-SIMD.md), [RGBA8 Sub](WARM-PNG-SUB-PREFIX.md), [stored RGBA16 encode](WARM-PNG-STORED-CAPACITY.md) and [RGB16 Paeth](WARM-PAETH-SIX-CHANNEL.md) parity confirmed; [tight RGBA streaming](WARM-PNG-STREAMING-ROWS.md) improves four gaps; other applicable proofs reused; precise selectors/allocations in the [summary](../docs/benchmark.md) |
| B14 | JPEG4208/16,restart,gray and geometric live-token/forward output; VP8 ordinary/simple/alpha/live-token/Stream and opaque8/16 encode; VP8L native/indexed/live-token/Stream and exact backend encodes | [VP8 source views](WARM-VP8-SOURCE-VIEWS.md) improve four encodes; [fixed JPEG encodes](WARM-JPEG-FIXED-AC-FUSION.md) and other unchanged proofs reused |
| B15 | Nearest Down8/16,Up8/16: unchanged early dispatch/integer kernels and allocation path | Existing independent proofs reused |
| C01 | PNG resident Identify: full container/CRC/EXIF without inflation, fresh descriptor; decode/cold timings do not cover it | [Closed](WARM-PNG-IDENTIFY.md),1.234x/upper1.238x,88B/op |
| C02 | JPEG resident Identify: complete marker/scan/stuffing/EXIF/EOI scan without entropy/pixels | [Prior-production confirmation](WARM-JPEG-ENTROPY.md),0.499x/upper0.506x,88B/op; reconfirm after four-component header support |
| C03 | WebP resident Identify: distinct RIFF/feature/header/EXIF algorithm, one existing VP8L packet | [Closed](WARM-WEBP-IDENTIFY.md),0.0040x/upper0.0045x,88B/op,explicit FFmpeg parser reference |
| C04 | Fresh non-exposed seekable Stream Identify on the same C01 PNG: fast metadata boundary exposes input acquisition/read/EOF/ownership costs | [Closed](WARM-PNG-STREAM-IDENTIFY.md),1.445x/upper1.459x,184B/op |

Historical <=2x audit gaps were resolved: stored16 PNG2.219x→1.090x, wide-cache
bicubic2.121x→passing current-path evidence, PNG Identify2.797x→1.234x,
JPEG Identify33.725x→1.840x, and Stream Identify7.863x→1.445x. Each production
round retains correctness/control evidence, short mean before confirmation,
all actuals/quiet guards and full accepted-change qualification. Raw refusals,
failed guards and superseded experiments remain in their original reports.

Historical [stored-capacity reconciliation](warm-png-stored-capacity-evidence.json)
replaces only the two storedRGBA8/16 encodes: word independent0.861x/upper0.870,
1,140,504B/op; byte1.295x/upper1.430,604,319B/op qualified short. Capacity allocation
is inside the complete fresh-output operation; packets, packing, compression,
checksums, work/working admission, writes/callbacks, limits and owners remain.
Other205comparisons/all59previouspasses are reused. Full3701+65scalar/OS and the
same actual package consumer/all3examples bothOS pass. No added case/grid.
60passes/143gaps/4confidence;145selected independent+62short. Passing word stops.

The preceding [Sub-prefix reconciliation](warm-png-sub-prefix-evidence.json) replaces
only DecodeSub8:50.5% faster,independent0.851x/upper0.855,4456B/op. Stride4 Sub
uses exact SIMD byte prefixes with unchanged resources/work,cancellation and
partial-failure behavior. Other206 comparisons/all58 previous passes are reused;
word Sub/byte Paeth control means remain within1.2%. No newcase/raster/settings
grid or profile. Full3692+92scalar/OS and the same actual package consumer/all3
examples bothOS pass.59passes/144gaps/4confidence;144selected independent+63short.

The preceding [packed-weight reconciliation](warm-bicubic-weight-broadcast-evidence.json)
replaces only Tight64x32_8 with a qualified short:12.5% less time,1.527x/upper1.797.
One exact vector-construction expression changes in the >20/≤36tap byte packed
core; all arithmetic/order, guards, workspace, work/cancellation and owners remain.
Generic join first regressed78.7%; retained JIT evidence identifies a full-width
stack dependency. Direct insert removes it, with no new profile or settings grid.
Unchanged controls retain their selected evidence; other206 rows/all58 passes
are reused. Full3680+138scalar/OS and the same actual package consumer/all3examples
bothOS pass.144selected independent+63qualified short; full parity remains open.

The preceding [live refinement reconciliation](warm-jpeg-refinement-lookahead-evidence.json)
replaces only ProgressiveCancelable444:10.1% less time, still1.552x/upper1.731.
Bounded ordinary lookahead retains original logical cursor, pending bits, per-bit
cancellation/work and immediate updates on every exit. Original fused/scalar
readers are untouched; two methods and one dispatch are added. Other206 rows and
all58 passing proofs are reused; unchanged ordinary-progressive/baseline controls
are within0.9%. Full3680+475scalar/OS and the same actual package consumer/all3
examples bothOS pass. All three short jobs retain actuals/both quiet guards on one
immutable producer/harness/selector/reference build. No new performance matrix.

The preceding [general byte-row reconciliation](warm-bicubic-byte-emitter-evidence.json)
replaces those same thirteen cache rows without new coverage. Five byte paths
improve11.6–20.8%; one dispatch/depth predicate and private name change, with the
entire emitter body unchanged. VerticalUp8's1.010x short warrants one independent
uncertainty check:1.020x/upper1.024x, a gap. Other12 remain qualified short gaps;
no unchanged repeat until pass. Other194 comparisons/all58 passing proofs retain
paths. Full3663+209scalar/OS and the same actual package consumer/all3 examples
bothOS pass. Native workers/gates and full boundaries are unchanged; no new
profile, raster, selector build or settings grid.

The preceding [byte-row reconciliation](warm-bicubic-byte-row-evidence.json) replaces
exactly thirteen existing default-cache timings, conservatively including word
and cross controls sharing the changed hot method. HorizontalUp8 improves16.4%;
all remain qualified short gaps. One suspicious vertical control is checked once
against the frozen predecessor:0.94% mean difference rather than4.14% against
older evidence. All actuals/guards retained, no repeat-until-pass. Other194
comparisons/all58 passing proofs retain their paths. Full3663 Release tests/OS,
209 scalar cache/reference checks/OS and the same actual package consumer/all3
examples bothOS pass. Original arithmetic, guards, allocation/work and approved
native policy remain. Two vertical reference identities reuse existing source
proofs without rebuilding. No new performance selector, input or settings grid.

The preceding [cancelable JPEG reconciliation](warm-jpeg-cancelable-ac-cache-evidence.json)
replaces only NativeDecodeCancelable8 with a qualified short gap after 9.3% less
managed time. One local cache method changes; prefix/amplitude work, cancellation
and partial-state semantics remain exact. Other206 comparisons/all58 passing
proofs retain their paths. Full3662 Release tests/OS,96 scalar JPEG checks/OS and
the same actual package consumer/all3 examples bothOS pass. The current approved
ordinary native444 worker is reused with original packet/output hashes and
unchanged complete boundaries; direct-RGB mode additions do not apply here.
No new performance image, selector or settings grid.

The preceding [byte bilinear reconciliation](warm-bilinear-byte-factorization-evidence.json)
replaces exactly Up8 with a qualified short gap after 16.8% less managed time.
Exact integer factorization preserves every final sum, original rounding and
admission. Only byte bilinear enlargement on both axes executes the changed
helper. Other206 comparisons/all58 passing proofs retain their paths. Full3659
Release tests/OS,132 scalar bilinear/reference checks/OS and the same actual
package consumer/all3 examples bothOS pass. Reference, quality and complete
operation boundaries are unchanged; no new performance input or settings grid.

The preceding [enlargement-strip reconciliation](warm-bicubic-enlargement-strips-evidence.json)
replaces exactly seven timings: tight both-axis/horizontal/vertical byte and word,
plus default horizontal word. All are qualified short gaps after8.3–17.8% less
managed time. Only existing strip capacities change; reversing them recovers both
entire predecessor files. Fixed arrays4000B remain within4096B admission; all
arithmetic, work, source/owner and exact-pixel/approved native gates are unchanged.
All200 other comparisons/all58 passing proofs retain their paths. Full3656
Release tests/OS,163 scalar resize controls/OS and the same actual package
consumer/all3 examples bothOS pass. Native horizontal identity reuse follows the
existing source proof; no reference build, new baseline policy or timing repeat.

The preceding [VP8L lookup reconciliation](warm-vp8l-borrowed-green-evidence.json)
replaces exactly seven decode timings with qualified short gaps: native VP8L
ordinary/live-token/Stream, literal/run, indexed16 and compressed-alpha VP8
ordinary/Stream. Native lossless improves13.5–13.7%; literal/run's mean is2.1%
above its preceding result and alpha controls are roughly flat. Original private
pixel tail scratch and stable grouping add no allocation/reservation/work.
All three predecessor files are recovered by removing only this primitive.
All200 other comparisons/all58 passing proofs retain their paths. Full3654
Release tests/OS,43 scalar Huffman/lookup checks/OS and the same actual package
consumer/all3 examples bothOS pass. Original validator refusals were reconciled
without repeating completed timings; no baseline/tolerance/boundary changed.

The preceding [adaptive JPEG reconciliation](warm-jpeg-adaptive-ac-evidence.json)
replaces all eight affected encode timings with qualified short gaps,9.9–14.8%
less managed time. One optional occupancy branch changes AC planning; removing
it restores the entire predecessor JpegEncoder file. All199 other comparisons
and58 passing proofs remain applicable. Packets, compressed sizes, approved
grayscale reference and quality/admission/complete-output boundaries are frozen.
Full3650 Release tests/OS,20 scalar adaptive checks/OS and the same actual package
consumer/all3 examples bothOS pass. No case was added or long gap confirmation run.

The preceding [JPEG scanner reconciliation](warm-jpeg-entropy-evidence.json)
replaces only resident JPEG Identify:0.499x/upper0.506x,72.9% less managed time.
One noncancelable branch/helper batches admitted entropy/stuffing; removing it
and three imports restores the entire predecessor JpegIdentifier file. All206
other comparisons and57 prior passing proofs retain their executed paths.
Full3648 Release tests/OS,20 scalar scanner checks/OS and the same actual package
consumer/all3 examples bothOS pass. The approved reference and all21 equivalent-
boundary controls remain unchanged. No case was added.

The preceding [byte packing reconciliation](warm-png-byte-packing-evidence.json)
replaces exactly gray8/RGB8/gray-alpha8 encode timings; all three independently
pass upper<=1 with exact packets and frozen Optimal9/native6/5% IDAT policy.
The entire predecessor PngCodec file is restored by removing one dispatch and
two packing helpers. All204 other comparisons and54 prior passing proofs remain
applicable. Current3646 Release cases/OS,36 scalar packing/admission controls/OS
and the same actual package consumer/all3 examples bothOS pass. No case was added.

The preceding [PNG Up reconciliation](warm-png-up-simd-evidence.json) changes only
filter2 dispatch and a safe SIMD row helper. DecodeUp8/16 independently pass
at0.984x/upper0.989x and0.995x/upper0.997x. The entire predecessor PngCodec file
is restored by removing that dispatch/helper; all205 other comparisons and52
prior passing proofs remain applicable. No admission/work/allocation or quality
change. Current3642 Release cases/OS, five scalar Up and127 scalar public PNG
controls/OS and the same actual package consumer/all3 examples bothOS pass.

The preceding [PNG chunk checksum reconciliation](warm-png-chunk-crc-evidence.json)
affects exactly16 existing PNG encode selectors. Ten independently reconfirm
upper<=1; six retain qualified short gaps. Stored8/16 means are1.624/1.664x;
the word gain uses a contemporaneous accepted-build control because its old
504.25us result was not reproduced. All packets/settings/quality/ownership gates
remain frozen. The other191 comparisons, including42 passing proofs, retain
unchanged executed paths. Decode/metadata Compute and its old helpers are
byte-identical. Current3,639 Release cases/OS,83 scalar CRC and49 scalar public
PNG controls/OS and the same actual package consumer/all three examples on
Windows/Linux pass. No material case or input was added.

The preceding [six-byte Paeth reconciliation](warm-paeth-six-channel-evidence.json)
adds one SIMD reconstruction kernel: RGB16 independently passes at0.959x/upper0.969x.
RGBA8/RGB8 dispatch controls have qualified short screens; their kernels remain
unchanged. The other204 comparisons, including all51 earlier passing proofs,
are reused. The stopped passing RGBA16 Paeth first branch is unchanged.

The prior [VP8 source reconciliation](warm-vp8-source-views-evidence.json)
changes only source conversion/sample access for four existing encode selectors.
Opaque8/16 are qualified screens at1.661/1.685x; alpha8/16 independently reconfirm
passing upper<=1. The other203 comparisons, including49 passing proofs, retain
their executed paths. No other codec, decode, metadata or edit path changed.

The prior [fixed AC reconciliation](warm-jpeg-fixed-ac-fusion-evidence.json)
changes the selected fixed writer traversal while preserving exact packets,
ordered work/callbacks, cancellation, quotas and partial state. Forward4208/16,
forward4448 and live-token4448/42016 have current qualified short screens at
1.399–1.548x; no independent parity confirmation is claimed. Adaptive coding,
general writer methods/fields, scalar fallback and all other codecs/edits retain
their paths. The other202 comparisons and all51 passing proofs remain applicable.

The preceding [cancelable refinement reconciliation](warm-jpeg-cancelable-refinement-evidence.json)
adds one post-fast-path live-token dispatch and a guarded block reader/helper/
wrapper. Previous methods and fields are unchanged. Only ProgressiveCancelable444
replaces its old timing: the qualified short screen is1.726x/upper1.798x, with
per-bit cancellation, immediate updates and exact work/failure state retained.
Ordinary progressive paths remain unchanged; subsequent encoder updates are above.

The preceding [paired-column reconciliation](warm-bicubic-paired-columns-evidence.json)
changes only the admitted word column-filter body and its checked row-offset
storage. Mixed16, TightMixed16 and TightVertical16 have fresh qualified quiet
screens; no independent parity confirmation is claimed while their means remain
1.307–1.462x. Other resize paths retain applicable proofs. No new case or raster
was added.

The preceding [fused refinement reconciliation](warm-jpeg-refinement-fused-evidence.json)
changes ReadBlock's refinement dispatch and adds a guarded reader plus small
helpers. All five affected progressive selectors have fresh independent
comparisons. Existing fallback methods, baseline JPEG, encoders, metadata,
Stream acquisition and other codecs retain their executed source paths and proofs;
the later three resize changes are recorded above.
The [preceding Stream reconciliation](warm-png-stream-identify-evidence.json)
still applies; its common acquisition boundary is represented once, with three
separate resident parsers. At that checkpoint, 3,619 Release cases per OS and consumer/all
three examples through the same actual Release package on Windows/Linux pass.

The original audit at349fedd reconciled203 numeric rows and fourteen historical
short regressions. Subsequent changes invalidate only the identified executed
paths; source/whole-DLL hash changes alone do not invalidate unrelated methods.
The linked B01-B12 reports record each old/new path and the accepted reuse or
replacement confirmation. Latest short regressions do not substitute for an
independent current-path proof when the timed production path actually changed.

Approved baselines, exact managed pixels, source-quality/size comparisons,
complete operation owners and quiet-VM confidence requirements remain unchanged.
Centered/clamped crop, AutoOrient and ResizeToFit use the named kernels after
small geometry work; correctness/composition controls cover those aliases.
The measured scope excludes GIF/animation, pending drawing/fonts, arbitrary profiles/
sizes/hardware, startup and concurrent throughput. Add a later case only when
concrete evidence identifies a distinct omitted required algorithm or public
boundary, with a named representative and reason. Do not expand settings grids
or retune a confirmed case without affected inputs or a demonstrated regression.
