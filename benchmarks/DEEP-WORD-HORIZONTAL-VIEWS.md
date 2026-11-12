# Deep word horizontal views

Bounded RGBA16 reduction now prepares exact-length horizontal tap views once per
column and a packed 64-bit sample view once per selected source row. Safe Span
indexing replaces repeated byte slicing. The unchanged x86 guard establishes
little endian word interpretation. Clamped offsets, unsigned premultiplication,
separate products/ordered sums, cache, certified rounding and repair remain exact.
No owner, buffer, pool, cache, kernel family or public API is added.

| Complete 513×259→32×16 RGBA16 resize | Baseline → current µs | Prepared zimg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Public Resize, input + output + 4 KiB | 441.62 → 403.94 | 412.77 | 0.979 / 0.985 | 113 |

Independent confirmation saves **37.68 µs / 8.53% raw**,
8.57% over all actuals, and 8.45% after native normalization.
Filtered/all-actual raw and normalized confidence separate; native mean moves
-0.09%. The original standalone reference now passes/stops. The contemporary
baseline and preceding adjacent measurement both establish the gap; this is a
scoped control result, not a general shape/depth speedup.

A three-actual screen precedes one independent 12-actual baseline/candidate capture
on immutable builds. Every actual/native bracket remains. Managed complete public
preparation/filtering/repair, admission, independent output and disposal stay timed.
Explicitly labeled FFmpeg zscale's zimg backend retains graph/scratch, and times
source conversion/halo/filtering/quantization/output acquisition and release.
This is the retained-graph comparison, not equivalent fresh graph preparation.
Frozen inputs/settings, exact qualified/rational managed pixels and approved
native-only bounds pass. No numerical policy or reference relaxation.

Original source reconstructs after only view/load substitutions; 2,048 byte-basis
checks establish word interpretation. The actual shared 9,388-byte Resize caller
and 488-byte publisher remain identical after address/method-number normalization.
The 61-case catalog leaves only bounded 32×16 on this changed path: its default
counterpart selects unchanged cached execution first. Other 72 resize and unrelated
family costs reuse source-valid evidence. Main JIT 2,989→2,969 bytes is diagnostic,
not the CPU proof.

Allocations remain 113 B/op, explicit arrays 4,032 B under 4 KiB preadmission, with zero
retained operation scratch. Physical/native/JIT peaks are unmeasured. Both OSes
pass 6,512 full/live checks, 570 internal/791 public focused checks per normal/scalar
mode, all three required storage suites and SAME actual-package consumers/all
three examples. Disabled ISA paths reuse their unchanged guards/fallback proof.
The new package case verifies frozen word pixels, aggregate admission, cancellation,
recovery and surviving owners. Its first scalar refusal assumption was corrected
using explicit caller reservation; the same package/DLL was reused without repack.
One core/embedded build per OS, zero public/harness/native/fixture builds, one pack.

Current resize scope: 73 controls, 65 pass/zero gaps/eight uncertain. Stop this control;
resolve uncertainty before tuning another. Published releases are untouched and
publication stays postponed. Actual private package 0.2.1 is 455,796 B,
SHA256 `5c4d569cbb69d1474d15595c40fa444bc788375d43bb35cdcb5232c59fd2111d`.

[Evidence](deep-word-horizontal-views-evidence.json) retains source/module identities,
checks and the measured package. Raw `deep-word-horizontal-views-qualified.tar.xz`, SHA256
`6e6728b4febc29b14f9de9686a339950a883db882141d5bae65d971b3a6e0f44` (2,089,248 B/96 members), is mirrored/member-verified.
The [earlier uncertain production gain](DEEP-WORD-VECTOR-WEIGHTS.md),
[adjacent gap](DEEP-WORD-CURRENT-CONFIDENCE.md) and
[attribution](DEEP-WORD-PAIRED-LOOP-ATTRIBUTION.md) remain linked history rather than
current renderer prerequisites. All 289 required paths remain.
