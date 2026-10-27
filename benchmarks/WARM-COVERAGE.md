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

[The current summary](../docs/benchmark.md) retains 20 unchanged 0.2.0 controls,
refreshes the VP8 Stream decode cost and thirteen read comparisons, and
refreshes three compressed PNG controls on the latest qualified 0.2.1 producer,
retaining the unchanged independent stored-PNG path.
[Bulk stored publication](WARM-PNG-BULK-STORED.md) independently measures PNG16
NoCompression at 1.052× / upper1.067, exact 1,066,552 B and 1,741,709 B/op.
Short screens are not independent confirmations; this representative scope does
not complete the full 267-case goal. [Constant-time distance mapping](WARM-PNG-DISTANCE-SYMBOL.md)
independently measures PNG Fastest at 1.471× / upper1.504; all twelve actual
samples remain. [Effort2 baseline refresh](VP8-RESIDUAL-SUMMARY-DEFERRED.md) resolves
its current short to 1.583×; the proposed summary primitive is deferred/removed.
[Trellis baseline refresh](VP8-TRELLIS-STATE-REDUCTION-REJECTED.md) gives 1.611×;
the state-reduction candidate is rejected and removed.
[The population-window family](VP8L-WINDOW-POPULATIONS-DEFERRED.md) stays closed:
its short gain does not survive confirmation; occupancy/pair reuse is insufficient.
[Scored-plane histograms](WARM-VP8L-SCORED-PLANE-HISTOGRAMS.md) supersede affected
VP8L costs after [color-trial reuse](WARM-VP8L-COLOR-TRIAL-REUSE.md). Photograph
independently measures1.350×/upper1.371,112.763ms,6.4% less elapsed than its fresh
120.536ms baseline. Allocations stay20409400B/op; exact160032B packets. RGBA8/16/
async screen1.444/1.438/1.485× with unchanged29688704/29688704/29689088B/op and
exact183812B packets. BothOS5044 full checks,13internal/27public scalar and SAME
actual package/three examples pass. Diagram and compressed alpha skip color search.
Trellis6 at 1.611× is the largest displayed encoding point gap, followed by opaque
VP8 photograph at 1.599×. Preserve larger absolute-cost VP8L leads in the queue.
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

## Evidence invalidation

Regression/pixel/reference evidence and timings are separate:

- Fuel/ownership cleanup changed polling, codec framing and nested callers. Earlier
  end-to-end costs do not establish current performance. Bicubic extraction kept
  exact arithmetic but changed code layout; older costs require measurement.
- JPEG inverse/color changes invalidate decoder/caller output costs. JPEG entropy,
  MCU normalization and record replay affect encoding only.
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
prioritize substantial VP8 effort6/photograph/effort2 and caller-RGB420 gaps before
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
