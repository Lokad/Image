# Shared bicubic quartet execution

[Compact geometry](COMPACT-MIXED-BICUBIC-GEOMETRY.md) now owns Mixed8/TightMixed8
costs. The other13controls and this stage's exact arithmetic evidence remain valid.

Complete bounded RGBA8 enlargement improves **3,129.13 → 2,969.56 µs**, saving
**159.58 µs / 5.10% raw / 5.12% reference-normalized**. Filtered/all-actual raw and
normalized confidence intervals separate; native time moves +0.019%. The current
conservative upper **0.981 passes**. Allocation remains **116 B/op**. This is the
existing 513×259→1027×519 control and its declared repeated-reference boundary.

Both depths now share four-pixel vertical accumulation. Byte enlargement reuses
existing quartet/pair publication and removes its private fixed certificate.
Shared byte preparation uses the word path’s vector exponent/power/divisor strategy
with identical floating-point bit patterns. The established ≤16-tap error envelope
covers enlargement’s ≤5 taps. Internal fast acceptance can change; full division,
ordered multiply/add, exact repair, tiny-alpha replay and declared managed pixels
remain. Existing rational/qualified-original oracles and certified sample-half
overrides stay distinct from approved native float tolerances.

Fifteen affected existing controls refresh: **9 pass / 2 gaps / 4 uncertain**.
Full enlargement is 1027×519, horizontal 1027×259, vertical 513×519; mixed is
1027×129 and TightVertical8 is 513×129. “Tight” means input+output+4 KiB; others
use default local limits. Every result is independently owned at the input depth.

| Existing case | Managed µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Up8 | 2,940.49 | 3,048.98 | 0.964 / 0.971 | 116 | pass |
| Up16 | 2,722.27 | 2,874.11 | 0.947 / 0.952 | 4,264,573 | pass |
| HorizontalUp8 | 1,660.27 | 1,634.05 | 1.016 / 1.037 | 114 | uncertain |
| VerticalUp8 | 1,726.30 | 1,740.64 | 0.992 / 0.998 | 114 | pass |
| HorizontalUp16 | 1,519.68 | 1,460.51 | 1.041 / 1.075 | 114 | uncertain |
| VerticalUp16 | 1,514.07 | 1,669.01 | 0.907 / 0.914 | 114 | pass |
| TightUp8 | 2,969.56 | 3,040.59 | 0.977 / 0.981 | 116 | pass |
| TightUp16 | 2,708.74 | 2,875.36 | 0.942 / 0.951 | 4,264,573 | pass |
| TightHorizontalUp8 | 1,546.81 | 1,515.06 | 1.021 / 1.035 | 114 | gap |
| TightVerticalUp8 | 1,722.39 | 1,743.04 | 0.988 / 0.994 | 114 | pass |
| TightHorizontalUp16 | 1,461.52 | 1,455.57 | 1.004 / 1.035 | 114 | uncertain |
| TightVerticalUp16 | 1,537.84 | 1,668.35 | 0.922 / 0.938 | 114 | pass |
| Mixed8 | 881.81 | 866.91 | 1.017 / 1.022 | 114 | gap |
| TightMixed8 | 863.94 | 870.01 | 0.993 / 1.000 | 114 | uncertain |
| TightVertical8 | 491.73 | 627.26 | 0.784 / 0.818 | 113 | pass |

Horizontal word enlargement remains uncertain. Its native brackets move
1,545.61→1,460.51 µs and managed actuals 1,563→1,482 µs. One contemporary
original counter gives 1,758.93/native1,579.56 µs, upper1.129; the raw/normalized
comparisons do not support a regression. Its older passing point cannot establish a
causal loss. The evidence retains the paired counter and clock movement; the
headline gain applies to bounded byte enlargement. No unchanged candidate is
resampled for a favorable bound, and no broader content/shape gain is claimed.

The first consolidation passes all pixels but its primary short gives no gain
(3,136.52 versus original3,136.66 µs). Scalar exponent/margin construction in the
shared byte helper motivates the vector revision. A emits 4,291 B/248 B row stack;
B emits 4,159 B/408 B versus original2,060 B/200 B. Code inspections discard all
instrumented elapsed time. Greater stack adjustment is disclosed, not inferred
as a physical peak or charged heap allocation.

No owned capacity, cross-operation cache or retained scratch is added. The primary
local bound remains 2,667,616 B: 531,468 B source +2,132,052 B output +4,096 B work.
Existing horizontal plans/cache borrow unpublished output, retire before tail
overwrites, and release stack workspace before tiny-alpha replay. The unchanged
4 MiB idle pixel pool and governed bypass remain. Word full output payload is
4,264,104 B. GC totals, live ownership and retention are separate; physical/native
peaks are unmeasured.

Complete public Resize includes admission, preparation, materialization and
disposal. The explicitly labeled **zimg 3.0.6 backend used by FFmpeg zscale**
retains graph/input/scratch; halo, packed conversion, filtering, quantization,
independent outputs and release remain timed. This is the recorded backend
boundary, with single-CPU sequential execution and idle-v2 guards.

Windows/Linux each pass **6,183 full/live checks**: 2,918 internal, 2,317 public,
948 live differential. Affected 351 internal/747 public checks pass normal,
scalar, AVX2-disabled and AVX512-disabled modes. The actual shared kernel matches
20,480 ordered scalar lanes bit for bit; existing pixel, ownership, refusal,
cancellation and recovery controls cover both depths, strip tails and workspace
retirement. The **same actual private Release package and all three examples**
pass both OSes. Unchanged JPEG ISA evidence is reused.

Two main/embedded versions per OS, no production compile failures or benchmark/
native/selector/fixture builds. Broad qualification/package checks occur once per
OS. A missing named test-filter scope is corrected with focused reuse of the
first immutable assemblies; the second filter includes those cases directly.
No new benchmark image/settings grid. The renderer now owns these costs directly,
drops three superseded enlargement providers and unused history-only checks,
and retains unchanged cost providers where they still supply displayed rows.

[Evidence](bicubic-shared-quartet-execution-evidence.json) pins all actuals, input/
output identities, sources, producers, arithmetic premise, qualification and
reconciliation. Raw **bicubic-shared-quartet-execution.tar.xz**, 9,668,180 B/
400 members, SHA256
`19a823a7f1666a784b01950870652cef9c8e1411b5e5960595a67c884883b88b`,
is mirrored/member-verified and retains originals, both versions, actual package
and reproduction tools. A metadata-key recovery reuses that completed archive
without rebuilding or retiming. Earlier [attribution](BYTE-BICUBIC-ENLARGEMENT-CURRENT-ATTRIBUTION.md)
and [enlargement history](BICUBIC-SHARED-ENLARGEMENT.md) remain separate.
All 283 required paths remain; this is no whole-release refresh. Publication and
published packages remain unchanged.
