# Horizontal byte enlargement confidence history

[Horizontal preparation/execution](BICUBIC-ENLARGEMENT-HORIZONTAL-MODES.md)
supersedes these costs on changed production code. The following measurement and
its raw evidence remain history, not current parity proof.

At this checkpoint, the existing control independently remained a **gap**, conservative upper 1.031,
on the unchanged actual 0.2.1 package DLL. This is a measurement refresh, not a
production change or a speedup. All twelve enlargement controls remain in scope;
the other eleven then retained source-valid evidence.

| Complete public Resize | Lokad µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| TightHorizontalUp8 | 1,682.98 | 1,641.69 | 1.025 / 1.031 | 114 | gap |

Original resident 513×259 RGBA8 →1027×259 bicubic output, input+output+4 KiB.
Managed preparation, processing, exact repair, independent output acquisition and
disposal remain timed. The explicitly labeled zimg worker retains graph/scratch;
this preserves its original prepared-reference boundary, not fresh-graph parity.
Original frozen/rational managed samples, native max 1/RMSE 0.25 bounds, source and
ownership gates pass. Twelve actuals, both native brackets and conservative
filtered/all-actual intervals are retained; quiet CPU0 and idle-v2 guards pass.

The actual private package `4a66c486a7a528a5712f891523b0be5a71164b88dc254d4a5d017db16892ab90` and timed DLL
`d93309af02ed6cc138546c102adf1d478d1ae46c0b49876a29f142306fbadb50` are unchanged. Its previous 6,321 full/live checks
per OS and SAME-package/all-three-examples proof remain valid. Eight new small
rational/poison/low-alpha/shared-refusal/cancellation/surviving-owner cases pass on
restored source: 420 internal normal/scalar checks per OS, plus the eight-case
filter with AVX2/AVX512 disabled. SIMD-only direct checks run where supported;
public ownership/fallback checks run in every mode. No broad/package repeat.
Allocations stay 114 B/op, scratch allowance 4 KiB; result capacity/retention are unchanged.
Native/physical/JIT peaks remain unmeasured. Publication stays postponed.

[Evidence](enlargement-horizontal-current-confidence-evidence.json) pins this
single current result and reused qualification. [Capacity trials and sampling
attribution](ENLARGEMENT-IDENTITY-CAPACITY-TRIALS.md) preserve the unaccepted designs
separately. The summary renderer uses this current result only, not their checks.
Raw `enlargement-identity-capacity-trials.tar.xz`: 3,254,092 B/231 members, SHA256
`69f2b633a99de22ce5ea1b395517e2f0e08b6946be6415f7ed4ff1188be15834`, mirrored/member-verified with original reference dependencies.
