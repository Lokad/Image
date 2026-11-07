# Ordinary VP8 row-window experiment

VRW525 is unselected. Keeping the ordinary coefficient reader through a
token-partition row does not improve the complete controls. The qualified
[prepared-model implementation](VP8-PREPARED-COEFFICIENT-MODEL.md) is restored
byte for byte, including source, main/embedded assemblies and public bindings
on both platforms. The actual package is unchanged. No production change remains.

| Independent complete operation, twelve actuals each subject | Original µs | Candidate µs | Raw change | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Opaque VP8 decoding | 4,906.29 | 5,003.91 | +97.61 / +1.99% | 228,864 / 228,864 |
| VP8 + alpha decoding | 6,121.24 | 6,140.05 | +18.81 / +0.31% | 238,384 / 238,384 |

Opaque loss separates in both filtered/all-actual raw and normalized views;
reference-normalized regression is 1.89%, with native movement +0.097%.
Alpha has no confirmed gain. Its filtered raw loss does not separate in the
all-actual or normalized views: no all-view loss or equivalence claim.
Subjects run in opposite case order on the quiet VM, CPU0, with original
input/pixel/alpha/ownership checks, both native brackets and idle-v2 guards.
The preceding three-actual screens and every adverse observation are retained.
No completed measurement is repeated to seek favorable confidence.

The short alpha point initially looked promising against its preceding launch.
The contemporary unchanged baseline instead measures 6,121.24 µs versus the
preceding 6,237.83 µs; native movement does not fully explain that variation.
Both remain confirmed parity gaps. Preserve both captures; use contemporary
subjects for the incremental comparison, without choosing a favorable historical
baseline, claiming a historical gain or retuning a passing control.

The implementation retained the same strict 272-byte bound per block, checking
continuation from the already-prefetched cursor. It published before fallback
and partition switches. All model, arithmetic, refill, coefficient and live-poll
bodies were unchanged. One borrowed window crossed reconstruction work; no new
heap capacity, cache, session or retained state was introduced. Admitted/physical/
native peaks were not remeasured; allocation counts establish no peak reduction.

Earlier [applicability counts](VP8-VALIDATION-READER-ATTRIBUTION.md) identify
13,389 buffered initializations on the frozen **coding-validation input**; the
retained macroblock design removes 11,684, leaving 1,705 on that input. Its packet
differs from the frozen resident decoding packet. These counts do not establish
decoding-control applicability, predicted time savings or current CPU attribution.
This failure concerns this lifetime/layout implementation, not a permanent ban
on row scheduling or architectural experiments.

Each OS passes **43 internal / 239 public checks per normal/scalar mode**,
including three new multi-macroblock pixel/validation/cursor and tight-partition
controls. One core/embedded build per OS; unchanged harnesses, native workers,
selectors and fixtures are reused. Exact restoration reuses existing 5,875-check
and same-package qualification; no broad/package/restore rebuild is needed.
All 125 displayed comparisons retain their qualified evidence. This report is
not a renderer dependency because it supplies no displayed result.

[Evidence](vp8-row-window-experiment-evidence.json) records design, projections,
complete observations, decisions and restoration identities. Raw
`vp8-row-window-experiment.tar.xz`, SHA256
`0795f11c0d4f6e2834a66250c0af3d39a6cadd0a94de4c4253bf43942c9f362b`,
11,350,428 bytes / 193 members, is mirrored/member-verified with its pinned
prepared-model provider. Sources, scripts, commands, actuals, gates and both
subjects remain reproducible. Publication and the full 283-path objective remain open.

Next inspect shared dequantizer preparation: current decoding repeats clamp/table
lookups across subblocks, while native decoders prepare frame/segment values.
Reference source provides lifetime insight only. Preserve Lokad.Image's exact
clamp order and arithmetic; no implementation or table is copied.
