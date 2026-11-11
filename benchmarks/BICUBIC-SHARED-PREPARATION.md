# Shared bicubic preparation

For current cached costs and package proof, see the
[family status](WARM-COVERAGE.md#current-family-status). Measurements below retain
this stage's design, incremental effects and raw evidence as history.

[Cached-wide vertical quartets](CACHED-WIDE-SHARED-QUARTETS.md) record an
intermediate stage. The horizontal-quartet report above owns all current affected
wide/narrow costs; the measurements below describe the original preparation stage.

CBP780B adds caller-owned `BicubicResizePlan` and shares preparation/execution
with ordinary resizing. Eligible SIMD two-axis reductions retain horizontal
geometry and working rows. Other geometries, tight local limits and portable
hardware use ordinary exact resizing. Every call reads fresh source samples,
resets cached row identities and returns an independently owned image. There is
no global cache, pool or retained input/output. Dispose the plan after its users.

Quiet-VM measurements use the existing original 513×259 byte/word rasters, one
CPU, frozen outputs and unchanged native pixel gates. The four equivalent
comparisons below all target128×64. One-shot includes geometry/graph, input-plane
and working-storage preparation, normalization, independent output and disposal
on both sides. Prepared comparisons retain explicit state on both sides; all
source reads, filtering, exact repair, result allocation and disposal remain timed.
The reference is **zimg3.0.6, FFmpeg zscale’s backend**, not the FFmpeg CLI.

| Complete boundary | Lokad µs | zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| One-shot RGBA8 | 389.10 | 997.60 | 0.390 / 0.393 | 198,457 |
| Prepared RGBA8 | 318.85 | 368.80 | 0.865 / 0.870 | 137 |
| One-shot RGBA16 | 451.13 | 626.32 | 0.720 / 0.771 | 198,451 |
| Prepared RGBA16 | 379.55 | 435.66 | 0.871 / 0.888 | 137 |

Prepared plans retain198,432 B at either depth. In untimed actual-package
consumer checks, peak governed owned capacity is231,200/263,968 B with one
RGBA8/16 result, excluding borrowed input. Results retain32,768/65,536 B after
plan disposal. Windows package checks use the exact portable fallback and retain
zero plan storage. Creation GC allocation, physical/JIT-stack peaks and native
graph/total allocations are unmeasured. Native ready-state scratch is645,696 B;
that is not total native retention. Total GC allocation, logical capacity and
idle retention are separate costs. Preparation is excluded only in the explicitly
prepared boundary. These performance claims cover the stated controls.

The existing fresh-managed/prepared-native comparisons remain visible below.
The shared executor changes invalidate their previous timings on these16 paths;
current immutable-package confirmations replace them. Earlier mixed-direction
width-enlarging controls return through unchanged result-band executors. Bounded
fallbacks, other filters, enlargement and codecs retain source-valid evidence.

| Existing cached control | Output | Lokad µs | Prepared zimg µs | Ratio / upper | B/op | Status |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| Wide8 | 128×64 | 391.10 | 345.95 | 1.131 / 1.134 | 198,450 | gap |
| Wide16 | 128×64 | 387.81 | 404.95 | 0.958 / 0.968 | 198,446 | pass |
| Down64x32_8 | 64×32 | 369.85 | 391.42 | 0.945 / 0.961 | 197,683 | pass |
| Down64x32_16 | 64×32 | 368.19 | 381.57 | 0.965 / 1.004 | 197,677 | uncertain |
| Down32x16_8 | 32×16 | 310.72 | 378.03 | 0.822 / 0.838 | 197,300 | pass |
| Down32x16_16 | 32×16 | 308.43 | 412.58 | 0.748 / 0.757 | 197,296 | pass |
| Down16x8_8 | 16×8 | 293.37 | 453.15 | 0.647 / 0.652 | 201,205 | pass |
| Down16x8_16 | 16×8 | 312.55 | 496.01 | 0.630 / 0.634 | 201,200 | pass |
| Down8 | 256×129 | 477.22 | 418.42 | 1.141 / 1.187 | 199,978 | gap |
| Down16 | 256×129 | 466.46 | 599.77 | 0.778 / 0.806 | 199,955 | pass |
| Horizontal8 | 256×259 | 452.96 | 565.56 | 0.801 / 0.809 | 199,977 | pass |
| Horizontal16 | 256×259 | 564.93 | 619.57 | 0.912 / 0.921 | 199,933 | pass |
| Vertical8 | 513×129 | 513.28 | 588.96 | 0.872 / 0.923 | 367,676 | pass |
| Vertical16 | 513×129 | 444.63 | 590.06 | 0.754 / 0.763 | 367,597 | pass |
| Cross8 | 256×519 | 784.89 | 838.25 | 0.936 / 0.945 | 199,943 | pass |
| Cross16 | 256×519 | 818.49 | 872.96 | 0.938 / 0.950 | 199,891 | pass |

Thirteen pass, two remain gaps, and one is uncertain. Do not repeat unchanged
passing controls or substitute old passing costs for current uncertainty. The
existing4097×2051→1024×512 managed default/compact diagnostic screens20.40/21.89ms
with three actuals each; it makes no native-parity or historical-gain claim.

The first shared prototype exposed substantial narrow-word regressions. Its
vertical-word confirmation1117.22/native640.15µs also contradicted historical
evidence: the contemporary unchanged baseline is783.56/native589.38µs, not the
older511.76µs. All remain retained. Moving the unchanged word-row loop and
certificate out of the scheduler removes the prototype’s per-tap accumulator
spill in generated code. Current444.63/native590.06µs saves338.94µs/43.26% raw/
43.32% normalized versus that contemporary baseline; both filtered/all-actual
raw/normalized intervals separate. No broader speedup claim follows.

The tradeoff is explicit: ordinary wide-byte resizing rises379.27→391.10µs,
11.83µs/3.12% raw/4.93% normalized adverse with separated intervals. Its gap
against prepared zimg remains. Retain the coherent architecture for measured
word-row and prepared-use gains, allocation reduction and shared execution;
fresh-native passes do not erase the asymmetric observations.

Exact coefficients, arithmetic order, certified rounding, true halves, tiny alpha,
repair and cancellation statements remain. A source proof checks the moved row
bodies and unchanged other kernels. New public tests cover rational samples,
fresh source mutation, cancellation during exact repair/recovery, retained roots,
refusal cleanup, independent survivors, async users and state exclusion. Native
fresh-graph tooling preserves the original settings and produces the exact same
normal/scalar native outputs as the original retained workers. Managed numerical
contracts remain stronger than native float intermediates.

Each OS passes6,229 full/live checks (2935 internal,2346 public,948 differential).
Affected resizing passes362 internal/776 public per normal/scalar/disabled-AVX2/
AVX512 mode. The same actual Release package and all three usage examples pass
on both OSes. JPEG ISA and other unchanged qualification are reused. The two
new prepared public boundaries extend the finite inventory283→285; the two
fresh-native views reuse existing operations. Publication remains postponed.

[Current evidence](bicubic-shared-preparation-evidence.json) pins source/build/
fixture identities, current statuses, all brackets, adverse results and the
mirrored/member-verified `bicubic-shared-preparation.tar.xz` archive. Its scripts
and source snapshots reproduce the factory/row prototype, native lifetime adapter,
frozen harness and final checks. Use `cbp780b-measure.py candidate CASE confirm`
for existing boundaries and `cbp780b-prepared-measure.py CASE confirm` for the four
equivalent boundaries after the recorded preparation; do not rebuild unchanged
production or rerun completed captures for postprocessing failures. Historical
[preparation attribution](CACHED-BICUBIC-PREPARATION-ATTRIBUTION.md) remains a
diagnostic lead rather than a subtractive savings prediction.
