# VP8 trellis candidate preparation

Independent coefficient arithmetic now runs before the serial predecessor walk.
Prepare capped floors, the second-candidate mask and signed 64-bit distortion
deltas in 320 bytes of block-local stack storage. AVX2 handles independent lanes;
the portable scalar path follows the same arithmetic. Rate-dependent decisions,
strict ties, terminal scoring, contexts and traceback remain the original source.
Zero-block, cancellation and disposed-owner checks still precede preparation.

The stage retains a **3.98% scoped paired improvement**, with a smaller serial DP
body. Its actual standalone comparison saves **195.79 µs/1.68% raw** with separated
filtered/all-actual confidence; the reference-normalized 0.125% point improvement
is unconfirmed. Do not substitute the paired magnitude for the standalone result.

| Complete frozen diagram encode | Managed µs | Native µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Effort 6, contemporary original | 11,633.13 | 10,741.04 | 1.083 / 1.096 | 754,497 |
| Effort 6, current independent | 11,437.34 | 10,573.45 | 1.082 / 1.093 | 754,497 |
| Effort 5, current secondary control | 7,108.12 | 6,987.16 | 1.017 / 1.028 | 741,065 |

Both current controls remain gaps. Effort 5's +49.70 µs raw adverse point and
7.42% normalized point improvement have overlapping confidence intervals. Its
current evidence resolves the earlier uncertain parity measurement as a gap.
Two affected controls refresh; seven unchanged encoding paths reuse valid proof:
three pass, three gaps, three uncertain. One displayed numerical row and the
family summary change; 124 other numerical rows reuse valid paths.

Twelve alternating pairs of 16 complete encodes per subject independently compare
11,508.51→11,050.30 µs, a 458.21 µs scoped saving. Ratio 0.960189 has interval
[0.957139,0.963243]. This isolated assembly-loading harness retains the same frozen
pixels, Q75/effort 6, fresh output construction/growth, independent ownership and
disposal. Its process/loading population and allocation accounting differ from the
standalone BenchmarkDotNet/native-bracket process. Paired allocation is 754,344 B
per subject; it is not transplanted into the table's total-allocation figures.
The differing magnitudes remain explicit; neither establishes native-normalized
standalone gain or FFmpeg parity. No further unchanged sampling seeks separation.

The initial three-actual screen has a raw adverse point against historical costs
while native time rises about 9%. That contradiction motivates one contemporary
original twelve, then one independent candidate twelve. Native point subsequently
moves −1.56% between those subjects. Every actual, outlier, bracket and guard is
retained. No added image/settings grid, native worker or benchmark build is used.
Reference wrappers/private storage retain their documented setup; complete public
source processing, admission, fresh output growth/ownership and disposal stay timed.
Frozen complete packets remain exactly 6,818/7,026 bytes with exact reconstruction.

Additional heap capacity and retained storage are zero. Existing 128-byte borrowed
DP storage, 2,048-byte standalone scratch allowance, 16,384-byte macroblock allowance
and 20,192-byte rate-plan capacity remain unchanged. Actual generated Search has
600 bytes of stack subtraction and its nested preparation helper 112 bytes, plus
saved registers; these fit the audited allowances. Search code shrinks 2,236→2,103
bytes; the helper adds 825 bytes. These code/frame observations are not CPU proof.
Total GC allocation is unchanged; complete physical/admitted peak and native
allocation are not newly measured.

Windows/Linux each pass **5,750 full/live checks** (2,494 internal/2,308 public/948
differential), plus 69 internal/140 public checks per normal/scalar mode. Fifteen
internal/22 public checks also pass with AVX2 and AVX512 separately disabled.
BigInteger checks cover signed sample extremes, wide weights and borrowed guards;
432 native signed-fragment records and all 22 frozen sync/async packet tuples stay
exact. Cancellation, owner disposal, terminal refusal, recovery and independent
surviving outputs pass. One immutable main/embedded revision per OS is reused.

The SAME actual private Release package and all three usage examples pass both
OSes. Package `59347fea823346063820396625117a613cff5555af77a8331cde14a2398cc560` (419,345 B).
Windows main `f375bb46c858e9fafc1a3012a43b91d2e573030b13505b5eee283a5e35d5d25e`;
Linux main `0ab1e9d06abf2dc0930297a61b291a01dd01758005e1714c734775c3dde9ff2a`. Published packages are unchanged;
publication and a full actual-package release refresh remain postponed.

[Evidence](vp8-trellis-candidate-preparation-evidence.json) records source proofs,
original/current points, the scoped pair, unchanged-path reuse and package checks.
Raw `vp8-trellis-candidate-preparation-qualified.tar.xz`: 2,726,832 B/168 members,
SHA256 `0cb9428ef776647daea6a809c66e86d07f4e3e2997c1cd1d5109ccba85641430`, mirrored and member-verified with pinned hash-addressed
dependencies. [Price preparation](VP8-BOUNDED-PRICE-VIEW.md),
[earlier trellis experiments](VP8-TRELLIS-ARCHITECTURE-AUDIT.md) and
[epoch attribution](VP8-RATE-EPOCH-PRICING-EXPERIMENT.md) retain their own evidence.
All 283 material paths remain required; full warm parity is open.

## Current instruction attribution

A bounded two-second follow-up uses the current qualified prepared-prefix DLL
(`05f30275…`), the unchanged accepted trellis source and the cached complete effort6
pipeline. All original source/settings/quality/ownership gates, the exact 6,818-byte
packet and idle-v2 checks pass. No builds, new fixtures or statistical timing.
Instrumented elapsed values are discarded; the current table remains unchanged.

Of 1,953 instruction samples, 1,886 map to managed code: Search663,
PrepareCandidates72 and Apply53. Search remains the stronger lead. Hot offsets
include rate-row bounds/addressing, predecessor scoring and terminal pricing;
these observations are not phase times or predicted gains. Search is 2,103 bytes,
preparation825 and Apply359. The older VTA398 capture predates accepted candidate
preparation and does not establish current instruction attribution.

The next bounded hypothesis prepares only the current block's fixed candidate,
predecessor-row and terminal prices before the serial recurrence. It must retain
arithmetic association, strict ties, terminals, traceback and exact packets, without
new heap storage or state escaping the call. Account for additional local capacity,
handoff and extra preparation. This differs from the rejected frame-epoch weighted
caches and equal-row/dead-state collapse; no gain or merge authorization is implied.
Use complete effort6/selected-effort5 callers to judge a prototype.

Raw `vp8-current-trellis-preparation-attribution.tar.xz`: 327,076 bytes / 32 members,
SHA256 `02bafe8eefd87f0f7541932ca20b6e7bced1495d3c8b9e50f4c2cf8cb2e613eb`, mirrored
and member-verified with pinned dependencies. A missing remote report caused a
preflight failure before profiling; its invocation is retained, then accepted
source hashes are pinned directly. No sample replay. Reuse current Windows/Linux,
scalar and actual-package qualification. The evidence records this as attribution,
separate from accepted performance results.

The subsequent [full-block price program](VP8-BLOCK-PRICE-PROGRAM-EXPERIMENT.md)
establishes no gain and restores this qualified source byte-exactly. Its larger
local storage and adverse three-actual point remain historical evidence outside
renderer checks; current production costs and qualification are unchanged.
