# Exact JPEG inverse-matrix SIMD

Selected JPEG420 costs are superseded by [integer chroma expansion](WARM-JPEG-QUARTER-CHROMA.md).
The inverse arithmetic and its qualification remain applicable.

The existing JPEG420 decode independently falls from6.086ms to3.965ms,
**34.8% less elapsed**. Exact reference pixels remain; **3.936×/upper3.992**
is still a gap to the warm single-CPU≤1× objective.

| Quiet run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Baseline, confirm | 6,085.790 | 1,042.051 | 5.840 / 6.398 | 757,166 |
| SIMD, short | 4,104.414 | 1,010.886 | 4.060 / 8.281 | 757,268 |
| SIMD, confirm | 3,965.366 | 1,007.556 | 3.936 / 3.992 | 757,268 |

Same513×2598bit source and frozen52,093B baseline420 reference packet. Complete
public Decode, independentRGBA8 ownership and release remain timed. FFmpeg's
softwareMJPEG decoder includes centered chroma expansion/crop, BT.601 conversion,
state reset and owned output/release; normal/scalar output is exact on this input.
CPU0, one codec/conversion thread, fixed optimizedJIT and nonconcurrentGC apply.

Eight matrix outputs/columns run in parallel with AVX2. Generated cosine weights
are transposed within the same256B table; moduloInt32 sums, saturation, DC shifts,
signed-word column bias and rounding stay exact. The portable fallback remains.
Explicit scratch stays256stack bytes, with no new owned heap buffer or admission
boundary. Cancellation still precedes mutation.

The33.7% short reduction clears the predeclared5% material gate. Independent
baseline/candidate each retain12actual/12filtered observations, separated
filtered/all-actual intervals and passing native brackets/idle-v2. These are
sequential means, not paired/general-content gains. Reported allocation rises
102B/op (0.014%, including harness overhead); native allocation and peak admitted
storage are unmeasured. No startup/concurrency claim is made.

BothOS pass5054full checks (1990internal/2117public/947live),39internal and293public
scalar checks, plus the SAME actual Release package and three consumer examples.
Two added controls freeze32original full-range integer spectra against preceding
scalar pixels and verify live cancellation/zero-admission/output integrity.
Existing frozen inverse/planar/public decoder anchors remain. Production builds
once perOS reuse the verified harness,workers,selectors and fixture; no scoring
is repeated after postprocessing recovery.

AllJPEG render/caller-sample decoder costs are affected; only this named cost is
refreshed here. Validation skips inverse rendering; its applicable baseline420
pass remains, as do unchanged encoding, other codecs and editing/text proofs.
[Evidence](warm-jpeg-simd-inverse-evidence.json) retains every actual/bracket,
filtered/all-actual bound, guard, identity, allocation and the mirrored/member-
verified112-member raw master. Published packages stay immutable.

Stop matrix variants. The next substantial primitive is exact quarter-phase
chroma expansion on the existing half-resolution path; the prechange diagnostic
identified that cost separately. Preserve edges, rounding and general fallback.
The full267-path warm single-CPU conservative≤1× objective remains open.
