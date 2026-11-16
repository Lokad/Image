# Batched font-header validation

HBC1157 accepts four-header SIMD validation in complete supplied-font loading.
Independent controlled pairs on the final immutable DLL save **1.150 µs / 6.05%**:
19.001 → 17.852 µs, ratio interval **0.935076–0.943933**. Allocations remain
**176 B/op**, with no added owned or retained capacity. These paired costs measure
the incremental implementation effect; they are not standalone reference parity.

The standalone complete copy/open/retain/dispose comparison remains a scoped gap:
**16.457 / FreeType 2.13.2 13.532 µs**, ratio **1.216**, interval
**1.134347–1.224863**. Managed eager header validation and FreeType's lazy
validation differ. The frozen input is DejaVu Sans 2.37 Regular, 757,076 bytes,
6,253 glyphs, SHA-256
`7da195a74c55bef988d0d48f9508bd5d849425c1770dba5d7bfc6ce9ed848954`.
This does not establish general parser equivalence or an overall text slowdown.

## Implementation and contracts

The noncancelable loop classifies four bounded glyph spans, loads their signed
bounds and performs one AVX2 comparison. Empty and zero-contour records retain
their original treatment. Any failed group returns to the original one-glyph
validation order, preserving the earliest public error. This is stateless input
classification, not an admission retry. The original cancelable loop checks
cancellation before every glyph. AVX2-disabled and scalar paths remain supported.
There is no new allocation, cache, owner, public API or numerical policy.

Original-order oracles cover 16,384 cases per DLL/mode, including short/long loca,
empty and truncated records, signed extremes, group boundaries and competing
errors. Both platforms pass normal, AVX2-disabled and scalar checks. Twelve new
public-operation regressions cover mixed records/tails, independent copied input,
refusal/release/recovery and error ordering. Existing cancellation and font
correctness checks remain unchanged. Generated code confirms fused signed bounds
work; instrumented clocks are discarded.

## Complete public text operations

Affected complete integrations were reconciled before broad qualification using
stock ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12. Every layout call
and all 30 fitting trials remain included, with fresh explicitly cleared RGBA16
canvases, independent outputs and disposal.

- One-shot: **1,136.908 / 6,217.742 µs**, upper **0.205748**, allocations
  **306,160 / 9,256,953 B**.
- Prepare once plus eight draws: **1,412.458 / 7,389.660 µs**, upper **0.214439**,
  allocations **315,892 / 9,494,633 B**. Directly measured totals amortize to
  **176.557 / 923.707 µs per draw**, including preparation.

Both pass. Preparation-only and prepared-draw timings remain scoped diagnostics;
no pipeline gain is inferred by adding stages or subtracting historical clocks.
Managed prepared state retains exact coverage while the reference retains paths;
font validation and GC-driven reference release also differ. Full 16-bit samples,
exact managed glyph/cluster/line/fitting checks and existing position/bounds and
visual gates all pass unchanged. Retained fit capacity **4,160 B** and governed
peak **822,460 B** are unchanged. Approximate full-GC retained heap deltas
**4,336 / 22,504 B** exclude idle pools and are separate from allocations and
unmeasured physical/native/JIT peaks.

## Qualification and reproducibility

Windows/Linux each pass **6,772 full/live checks** (3,316 internal, 2,508 public,
948 differential), plus **269 internal / 441 public** focused checks per normal,
AVX2-disabled and scalar mode. The SAME actual Release package passes its consumer
and all three usage examples on both platforms. Published packages are unchanged;
publication remains postponed.

The timed/package Linux DLL is
`02c8a14900b52d4da539619fcc50586e76cf79b2053e594a7bbb909b078ab960`;
Windows DLL `8a5264ab733af398a6bd6bd93fc2457298e1403e01bfb6b646b328f99a2ec50c`.
The private 0.2.1 package is
`267d5559c328d27a3a873d1aa8545df57dff42a0e12437c10b912b1e5120a712`
(469,041 bytes). Each final core and embedded test assembly was built once per OS;
unchanged public/reference/harness/fixture builds were reused. Packing used the
existing Release module. Nothing rebuilt the timed artifact.

The short three-pair screen led to independent twelve-pair prototype and final-DLL
confirmations. All samples, reference brackets, output checks and idle-v2 guards
are retained. A final preflight source-pin assertion stopped before workers or
samples; pins were reconciled to the promoted source and retained baseline, then
the original experiment resumed without rebuilding or repeating measurements.
The noncancelable-loop building block's small earlier effect is not added.

[Evidence](font-header-batching-evidence.json) identifies modules, sources,
reference identities, memory, qualification, invalidation and reproducible recipes.
Its mirrored member-verified `font-header-batching-qualified.tar.xz` retains the
actual package, both platform modules, prototype/baseline, scripts, ordered
oracles, code generation, every actual and qualification log. Referenced raw
dependencies retain the original workers and fixtures.

Borrowed ordinary/prepared/authored drawing loads fonts outside its timers;
those paths and codecs/resizing/copies are unchanged and are not retimed.
Earlier shared-storage costs retain identified historical artifacts pending the
final complete immutable-package refresh. The finite 289-path objective remains
open; no new font, image or settings grid was added.
