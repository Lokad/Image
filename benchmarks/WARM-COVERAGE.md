# Material warm coverage

The full objective remains independent quiet single-CPU warm performance with a
conservative ratio upper bound ≤1× for every named material required path, plus
functional, memory, compression and packaged-consumer correctness. Startup and
concurrency are excluded. Compression correctness precedes CPU optimization.
The published 0.2.0 snapshot is retained as the baseline; current evidence is
reconciled in the summary below.

## Finite checklist

The [immutable inventory](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/benchmark-pre-cleanup.md)
names 257 controls. Remove four retired boundaries and add two distinct required
algorithms: **255 existing required cases**. Twelve named controls below cover
the newly added read boundaries: **267 total**, without a shape/depth/settings grid.

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

[One-pass PNG validation](WARM-PNG-VALIDATION.md) independently closes the PNG16
span and async cases:0.990×/upper0.99966 and0.833×/upper0.847. Span elapsed falls
42.4%; allocation240B/op, async8544B/op. Keep the narrowly passing span unchanged;
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
All remain gaps; no independent timing confirmation or speedup claim. Current
production qualification/package proof is reused. These six controls do not
refresh every Identify/decoder boundary affected by read-policy changes.

## Current evidence

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

## Evidence invalidation

Regression/pixel/reference evidence and timings are separate:

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
Closed source-energy, histogram,
word/fragment, prediction/quantizer/trellis and other rejected variants stay closed.
Use immutable builds and narrow jobs; batch full Windows/Linux/scalar/package
checks for accepted production changes. Preserve complete operation boundaries,
exact pixels/packets, shared admission and quiet-VM confidence requirements.
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
