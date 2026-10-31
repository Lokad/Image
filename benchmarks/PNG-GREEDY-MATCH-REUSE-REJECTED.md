# Greedy PNG long-match reuse: rejected

One short screen fails the retention gate: **23,988.59 µs** versus the current
independent **23,284.98 µs** baseline, **3.02% higher filtered/all-sample elapsed**
and **2.96% higher normalized point ratio**. Allocation stays **3,813,732 B/op**.
This is a failed screen, without an independently confirmed regression claim.
No confirmation, variant or additional profile follows; accepted costs stay.

The trial verifies the first eight bytes in greedy level1 search, then calls the
unchanged qualified long-match helper already used by lazy level6/9. Ring edges,
overlap, lookahead, chain order, ties, nice8/depth4/skip decisions and cancellation
remain. There is no new storage, cache, owner or capacity. The existing original
513×259 RGBA16/None/level1 control keeps its complete **968,216-byte packet**
exactly, as well as every sample, alpha/hidden RGB and source/output ownership.
Native reference/settings, complete operation, four calls per actual and both
idle-v2 guards are unchanged.

One current diagnostic profile has **776 workload / 755 managed samples**;
ProcessFast accounts for 533 exclusive samples (68.69%). Method attribution
motivates the bounded trial but does not prove long comparisons dominate; these
fractions are not component elapsed times. All instrumented times are discarded.
Trace SHA256 `b66e26b4c6f0e34b9de2a2c12cc7d63a0eb1f580d5906557c76d834b3fe0396e` is retained.

CF16's just-recorded original twelve is reused, with matching source/core and
frozen input/settings/boundary pins. One candidate three, one immutable core build
per OS, zero harness/native/internal/public/differential assembly builds. The
cache has no suitable dynamic four-call short driver, so one tiny selector-only
build changes twelve iterations to three and preserves all four generated calls,
InvocationCount4 and UnrollFactor1. The cached sampler is reused.

All **201 public PNG checks** pass normal/scalar on both OSes for candidate and
restored production, including frozen packets, limits, shared admission and
cancellation. Source and core/public DLL/PDB/XML are restored **byte for byte**
on both OSes, with zero restoration builds and no new tests. The same actual
Release package is unchanged; existing **5,332 checks per OS**, both actual
package consumers and three examples remain applicable. No repeated full/package
qualification for rejected production. Close this caller-transfer family.

[Numeric evidence](png-greedy-match-reuse-rejected-evidence.json) retains actuals,
confidence, profile and restoration. Mirrored/member-verified raw
`png-greedy-match-reuse-rejected.tar.xz`, SHA256
`2be4f5684e6c07034568c9691263dd417eb265c49a0bc54f22c5c4d0fc4001a0` (5,350,232 B / 121 members),
contains original/candidate source/cores, cached tools and driver source/build,
frozen fixtures/worker, trace, gates/logs and reproduction scripts. CF16/SKP1 raw
dependencies remain. Native allocation is unmeasured. The full **268-path warm
single-CPU upper ≤1×** objective remains; exact packets/pixels take precedence.
