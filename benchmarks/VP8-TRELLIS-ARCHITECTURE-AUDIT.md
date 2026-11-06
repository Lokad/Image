# VP8 trellis workspace audit

Neither workspace implementation establishes a complete-operation gain. Both are
restored to the qualified implementation without rebuilding. Exact packets pass
throughout; no compression or production API change is accepted.

VCB421 borrows the unchanged coefficient input until traceback and compacts the
scratch buffer from 64 to 48 shorts. VPN422 revises that design: private trial-level
slots temporarily hold the floor, sign and two predecessor bits, eliminating the
separate scratch span and three stack allocations. All original arithmetic,
strict ties, cancellation checkpoints and final signed levels remain unchanged.
Three callers have disjoint coefficient/level buffers; no partial candidate escapes.

| Complete existing diagram/Q75 encode | Original µs | Candidate µs | Raw change | Allocation B/op |
| --- | ---: | ---: | ---: | ---: |
| VCB421 effort5, short | 7,384.12 | 7,288.88 | −1.29% | 741,065 |
| VCB421 effort6, short | 10,887.04 | 11,039.44 | +1.40% | 754,497 |
| VPN422 effort5, short | 7,384.12 | 7,072.91 | −4.21% | 741,065 |
| VPN422 effort6, short | 10,887.04 | 11,194.15 | +2.82% | 754,497 |
| VPN422 effort5, independent12 | 7,384.12 | 7,265.17 | −1.61% | 741,065 |
| VPN422 effort6, independent12 | 10,830.23 | 11,521.55 | +6.38% | 754,497 |

All raw and native-normalized confidence intervals overlap, in filtered and
all-actual views. The effort5 original reuses the applicable VCS420 twelve;
effort6 receives one contemporary original twelve because its original three
actuals are too uncertain. Effort5 confirmation filters two candidate outliers;
all twelve remain in the evidence. Native point movement for the independent
comparisons is −0.13%/−0.67%; normalized point changes are −1.48%/+7.10%.
These are inconclusive comparisons, not established gains or regressions.
Stack simplification without an allocation saving does not justify retaining
this implementation's adverse effort6 point. No unchanged favorable repeat or
permanent closure of materially revised designs follows.

Complete public operations retain normalization, preparation, shared admission,
fresh output growth, independent ownership and disposal. The native worker retains
its documented wrappers/private storage. Frozen effort5/6 packets remain
7,026/6,818 bytes, with exact reconstructed pixels, alpha and source ownership.
Each run uses CPU0, idle-v2 guards and both native brackets. No new images/settings,
harness, worker, selector or reference builds occur in either prototype.

Each version's immutable Windows/Linux main and embedded test assemblies passes
142 internal/137 public checks in normal and scalar modes. The golden independent
residual corpus, signed zero/DC/tails, cancellation/disposed-owner and frozen packet
checks pass. All 32,760 packed-node mappings are checked separately as representation
applicability, not a performance or complete behavioral proof. Each prototype
builds only main/internal once per OS. No broad/package qualification is wasted
on either unaccepted version. Exact restoration reuses the qualified 5,742
full/live checks per OS and same actual package/all three examples.

The restored effort6 twelve is current read-only evidence:
**10,830.23µs / 9,936.97µs native, ratio1.090, lower0.967 / upper1.130,
754,497B/op**. It is uncertain, superseding the older confirmed gap. This is
not a causal optimization claim. [Current evidence](vp8-trellis-boundary-refresh-evidence.json)
is independent of rejected-implementation assertions in the summary renderer.

A single two-second VSA423 capture attributes the distinct required effort5 path
on restored production: 1,944 total/1,902 mapped managed samples, including
214 Luma4, 173 plain full-block quantization, 153 reconstruction, 127 Luma16 and
64 trellis-search samples. These are instruction samples, not phase times or
predicted gains. No statistical timing is taken from the instrumented run.
Next inspect repeated plain matrix preparation and reconstruction across candidate
blocks, accounting for existing prepared-Luma4 and rejected pair experiments;
do not replay the DP packing or infer a win from fewer stack bytes.

Raw `vp8-trellis-architecture-audit.tar.xz`: 2,221,680 bytes / 220 members,
SHA256 `73f1d0fd787fd732f1aa8146f78ee58be6dd4c856fcd6547fead6e0189869fd4`,
mirrored and member-verified. It retains designs, source/binary snapshots,
focused logs, every actual/bracket/guard, restoration, profile/JIT/map/collector,
commands and hash-addressed dependencies. VCS420's earlier control archive remains
`923340a5011063d8196bb6fff05ca95923388ff024abb7bf80159e005e1a8f79`.
Published releases and the qualified private package remain unchanged; all 283
required paths remain in scope. Publication is postponed.
