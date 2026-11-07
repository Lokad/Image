# Fused VP8L inverse transform and publication

LPF476, 2026-11-07. Accepted scoped tradeoff; publication postponed.
Global/package qualification is superseded by
[completed-block publication](VP8-COMPLETED-BLOCK-PUBLICATION.md); all nine reader
costs and the palette tradeoff remain source-valid.
[Current evidence](vp8l-final-publication-evidence.json) owns nine existing
VP8L/ALPH decoding costs. The full 283-path performance objective remains open.

## Design and exactness

The final subtract-green inverse and subsequent ARGB-to-RGBA publication previously
made separate scalar passes over the complete raster. They now share one pass,
using byte shuffles and wrapping addition on AVX2/Ssse3, with a scalar tail.
Only the final inverse (index zero, subtract-green) may publish, and only for full
RGBA decoding into an already-owned little-endian, full-width result. A private
output-purpose enum and publication flag make that representation change explicit.
Earlier transforms, auxiliary images, palette/big-endian fallback, ALPH green
extraction and validation retain their existing arithmetic and representation.

No heap buffer, pool, cache, session, reservation or escaping owner is added.
Existing result allocation precedes decoding; independent output remains charged
until disposal. Transform cleanup and failure ownership are unchanged. Cancellation
checks occur at most 256 pixels apart and at completion. Exact wrapping, alpha and
hidden RGB remain mandatory. A mechanical source projection reproduces the entire
qualified original decoder outside these changes; counts are not timing evidence.

## Complete warm operations

Dedicated quiet VM, Release, one pinned CPU, original frozen inputs and native
workers/settings. Each confirmation contains twelve managed actuals and twelve
samples per native bracket, passing idle/process guards and original output,
source, alpha and ownership checks. Native contexts remain retained as declared;
complete managed preparation, independent output ownership and disposal stay timed.

| Existing control | Managed µs | Reference µs | Ratio / upper | Managed GC B/op | State |
| --- | ---: | ---: | ---: | ---: | --- |
| ValidateVp8lAlpha | 2,309.16 | 3,044.52 | 0.758 / 0.765 | 297,070 | pass |
| ValidateVp8Alpha | 4,646.03 | 5,440.02 | 0.854 / 0.860 | 13,584 | pass |
| WebpResident | 3,093.70 | 3,008.39 | 1.028 / 1.034 | 152,728 | gap |
| Lossless8 | 1,524.08 | 2,154.96 | 0.707 / 0.714 | 10,840 | pass |
| IndexedNative16Colors8 | 574.97 | 542.81 | 1.059 / 1.072 | 820,192 | gap |
| NativeLosslessCancelable8 | 3,149.12 | 3,036.05 | 1.037 / 1.043 | 152,728 | gap |
| WebpStream | 3,090.59 | 3,027.80 | 1.021 / 1.027 | 152,824 | gap |
| Native8 | 6,237.13 | 5,966.87 | 1.045 / 1.049 | 238,384 | gap |
| Vp8Stream | 6,229.92 | 5,995.22 | 1.039 / 1.043 | 238,480 | gap |

Independent resident original/candidate confirms **3,249.36→3,093.70µs**, saving
**155.66µs/4.79% raw**,4.53% native-normalized; native moves−0.274%.
Filtered and all-actual raw/normalized intervals separate. GC remains152,728B/op.
The preceding short screen is separate evidence, not parity proof. Resident,
live-token and stream still have demonstrated gaps; passing cases stop.

The palette control unexpectedly exceeded older costs. One contemporary original
confirmation resolves that concern: **555.72→574.97µs**, a **19.25µs/3.46% raw
regression**, with native essentially stationary. Both raw/normalized confidence
views separate. Fusion is ineligible there; the specific compiled-call-path cause
is not established. Accept the bounded tradeoff for the larger resident saving,
retain the adverse result and prioritize larger absolute gaps. Do not claim a
general decoder speedup, equivalence elsewhere or favorable resampling.

## Storage and qualification

Fresh GC totals are in the table. Owned capacity/lifetimes are unchanged by the
verified source projection: no new retained workspace, and the full raster
is531,468B until its owner disposes. Existing public/package refusal, cancellation,
recovery, surviving outputs and async checks pass again. Validation's coarse
235,152/11,121B peaks and zero retention reuse unchanged ownership evidence from
[validation elision](VP8L-VALIDATION-ELISION.md); they are not new peak measurements.
Default capped pixel pooling and governed bypass remain unchanged. Physical/native
peak storage is unmeasured; do not infer it from GC totals.

One immutable core/embedded build per OS; cached public, differential, benchmark,
selector, native, diagnostic and fixture builds reused. **5,837 full/live checks
per OS** (2,572 internal/2,317 public/948 differential), plus49 internal/239 public
per normal/AVX2-disabled/scalar mode pass. Channel oracles cover vector/checkpoint
tails, wraparound, hidden RGB, canaries, cancellation and zero extra allocations.
The SAME actual private Release package and all three usage examples pass on both
Windows/Linux. Other codecs, encoders, resizing and text retain source-valid proof.
Five displayed costs refresh;120 numerical rows reuse unchanged paths. This is not
a full actual-package performance refresh or publication.

## Reproduction and retained evidence

Raw archive `vp8l-final-publication-qualified.tar.xz`, SHA-256 `744db18502923a4535d2ecfd4fd25bad76a6b9c32e3d1dc124615ce08bc26786`, 2040148 bytes/165 members,
is mirrored and every member verified. It retains source/originals, modules,
cached-input providers, short/independent/adverse comparisons, all nine current
costs/guards, source projection, Windows/Linux qualification and actual-package
logs. See its `tmp/lpf476-*.py` scripts and JSON receipts. Earlier
[VP8 output](VP8-OVERWRITE-OUTPUT.md) and
[VP8 candidate preparation](VP8-SMALL-BLOCK-PREPARATION.md) retain separate evidence.
