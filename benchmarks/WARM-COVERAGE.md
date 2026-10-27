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
algorithms: **255 required cases**, not an expanding shape/depth/settings grid.

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

## Current evidence

[The current summary](../docs/benchmark.md) retains 27 unchanged 0.2.0 controls and
refreshes three compressed PNG controls on the latest qualified 0.2.1 producer,
retaining the unchanged independent stored-PNG path.
[Bulk stored publication](WARM-PNG-BULK-STORED.md) independently measures PNG16
NoCompression at 1.052× / upper1.067, exact 1,066,552 B and 1,741,709 B/op.
Short screens are not independent confirmations; this representative scope does
not complete the 255-case goal. [Constant-time distance mapping](WARM-PNG-DISTANCE-SYMBOL.md)
independently measures PNG Fastest at 1.471× / upper1.504; all twelve actual
samples remain. VP8 diagram effort2 at 1.685× is now the largest displayed gap.
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
- Release documentation and package-document changes do not invalidate unchanged
  production behavior or applicable performance proofs. The release snapshot uses
  the exact packaged DLL and refreshes actual producer-bound frame/async proofs.

Frozen inputs/settings/workers, approved native-only adjustments and exact oracles
remain reusable when unchanged. Retired quota cases cannot be failures or passes.
Source/path changes must identify affected older evidence, not invalidate the
whole library by default. Older histories remain in
[the preceding coverage record](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/benchmarks/WARM-COVERAGE.md)
and their individual reports/raw archives.

## Next performance work

Resolve uncertainty before optimization; optimize demonstrated largest gaps and
stop confirmed passing cases. Stored publication and distance mapping are accepted;
their remaining gaps are below VP8 effort2 at 1.685×, the next displayed control.
Closed source-energy, histogram,
word/fragment, prediction/quantizer/trellis and other rejected variants stay closed.
Use immutable builds and narrow jobs; batch full Windows/Linux/scalar/package
checks for accepted production changes. Preserve complete operation boundaries,
exact pixels/packets, shared admission and quiet-VM confidence requirements.
