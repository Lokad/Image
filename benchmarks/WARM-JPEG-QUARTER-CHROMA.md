# Exact integer JPEG chroma expansion

The existing JPEG420 decode independently falls from3.965ms to2.095ms,
**47.2% less elapsed**, with exact reference pixels. Its **2.079×/upper2.241**
gap remains to the warm single-CPU≤1× objective.

| Quiet run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Qualified baseline, confirm | 3,965.366 | 1,007.556 | 3.936 / 3.992 | 757,268 |
| Integer chroma, short | 2,269.646 | 1,064.145 | 2.133 / 2.878 | 757,298 |
| Integer chroma, confirm | 2,095.452 | 1,007.974 | 2.079 / 2.241 | 757,298 |

Same513×2598bit source,52,093B baseline420 reference packet and complete Decode/
independentRGBA8/release. Native softwareMJPEG includes centered chroma expansion,
crop,BT.601 conversion,state reset and owned output/release. CPU0,one codec/
conversion thread,optimizedJIT and nonconcurrent workstationGC remain fixed.
Normal/scalar native output and every managed sample agree exactly.

Centered half-resolution chroma positions are exact binary quarters. Fixed1/3
weights and shared clamped row/column indices replace repeated floating-point
coordinates,floor and rounding; (sum+8)>>4 keeps the same single final byte round.
Full-height422 rows and odd/clamped edges retain their phase. The same64B stack
tiles,color conversion,caller ownership and parent cancellation checkpoint remain.
No new heap owner,cache,reservation or frame copy is added. Other sampling/color
paths keep their general interpolation.

The44.7% short reduction clears the predeclared5% gate. The valid independent
JIV1 baseline is reused; one new12actual/12filtered confirmation has separated
filtered/all-actual intervals and passing native brackets/idle-v2. No baseline
retiming or scoring repeats. These are sequential means,not paired/general-content
claims. Reported allocation rises30B/op (0.004%,including harness overhead).
Native allocation and admitted peaks are unmeasured.

BothOS pass5054full checks,including947live comparisons. Existing293public JPEG
checks perOS pass normal/scalar before scoring and are reused; unchanged inverse
scalar qualification remains. Frozen pixels cover padding,orientation,odd/fringe
edges and caller/stream/async/admission/cancellation/ownership behavior. The SAME
actual Release package and three examples pass bothOS. One corebuild perOS reuses
the harness,workers,selectors and fixture. Published packages remain immutable.

A six-second current-core profile (5780samples) places22.4% in chroma expansion,
20.2% in ACentropy,19.6% in the inverse kernel plus3.4% in entry/dequantization,
and19.3% in color conversion. It reuses the existing profile host with zero builds,
exact pixels and quiet guards. These are diagnostic shares,not timing/gain evidence.
The preceding profile predates two material changes. Next is one SIMD chroma tile,
keeping integer edges/tails and single-pixel caller writes; no profile repeat.

Affected YCbCr420/422 RGB/RGBA costs need current evidence before CPU-pass claims;
only this named case is refreshed. Luma-only callerGRAY,444,411/410,directRGB,
CMYK/YCCK,validation/Identify,encoding and other codecs/editing/text are unchanged.
[Evidence](warm-jpeg-quarter-chroma-evidence.json) retains all actuals/brackets,
filtered/all-actual bounds,guards,identities,allocations and the mirrored/member-
verified140-member raw master. Full267-path parity and general readiness stay open.
