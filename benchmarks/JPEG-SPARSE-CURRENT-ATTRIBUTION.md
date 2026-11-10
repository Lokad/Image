# Sparse JPEG inverse attribution

JSC767 reuses the current qualified package for one two-second sparse-CMYK
capture after the accepted token-descriptor change. Complete decode remains
484.96/native 437.68 µs, upper 1.121 gap, 20,944 B/op. This adds **no timing or
allocation result**, production build, fixture or qualification repeat.

Of 1,994 sampled locations, inverse AVX2 has 611, plain entropy 263, baseline
scheduling 205, dequantization 158, component packing 80 and CMYK conversion 79.
CPU0 executes and CPU1 samples; both idle-v2 guards, the frozen 22,708 B packet,
exact 531,468 B managed/native output and source/ownership checks pass.
Instrumented elapsed is discarded. Inlining/skid make these locations attribution,
not phase costs, removable percentages or predicted CPU savings.

The actual 1,772 B inverse body retains a useful two-low-row path. Its column
execution loads weights, checks/publishes reflected output rows and repeatedly
narrows complete vectors to scalar eight-byte stores. Mapped regions 20/22/23
have 207/136/66 samples, including skid. This identifies a larger coherent lead
than replaying the [unselected coefficient fusion](JPEG-COEFFICIENT-FUSION-EXPERIMENT.md).

## Bounded design lead

Investigate one exact batched column publisher for the existing two-row case,
with format-derived prepared weight pairs and retained AVX2/scalar fallbacks.
Signed-word bias must precede widening; pair products/sums, shift, clipping,
reflection and byte order must match every existing result. Preserve DC-only
shortcuts and cooperative cancellation. Preparing/retaining weights has a cost
that must be recorded; no caller budget, output lifetime or numerical contract
changes. No speed or allocation gain is assumed from fewer stores or checks.

A separate support classification reuses the existing independent T.81 parser
and exact dequantization on two frozen packets. All 8,580 non-DC sparse blocks
have top-left 2×2 support; only 2 of 6,630 RGB blocks do. On those sparse blocks,
three zero row-pair terms repeat 8,580 times, alongside intermediate row storage.
These logical counts may justify a later row-preparation extension; they do not
establish its cost or make a fixture-specific gate acceptable. Earlier fusion
counters are excluded from the current model, without another parse.

Start with actual publisher lane/extreme-word oracles and existing sparse/RGB
complete screens. Preserve word wrapping versus saturation at the distinct row
boundaries. New packing/eligibility, code size, register pressure or prepared
storage may offset the benefit. Revise or abandon based on complete evidence;
do not treat this profile as merge authorization.

Enumerate changed inverse callers before refreshing performance. Reuse unchanged
DC-only/validation paths where proved; retain all existing packet/pixel, input,
memory/admission, cancellation and surviving-output checks. Broad Windows/Linux,
affected scalar/ISA and same actual Release-package checks belong at a coherent
accepted stage. No image/settings grid is added.

[Evidence](jpeg-sparse-current-attribution-evidence.json) pins the current sources,
mapped instructions, support counts and actual package. Existing 6,181 full/live
checks/OS, affected ISA and same-package/all-three-example evidence are reused.
Mirrored/member-verified raw `jpeg-sparse-current-attribution.tar.xz`:
300,248 B / 33 members, SHA256
`3d1d2dcf7c467fc9f86564719c93f2494131f8b1859a3778e97d74b401635170`.
No benchmark-renderer dependency or displayed comparison changes. All 283 required
paths remain open for qualification; publication is postponed.
