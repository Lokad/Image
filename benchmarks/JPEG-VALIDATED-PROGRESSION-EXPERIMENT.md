# Validated JPEG progression experiment

JVC617 is unselected. Its sole complete live-progressive444 screen measures
**3,577.63µs**, versus the source-valid **3,553.06µs** baseline: an adverse
24.57µs/0.69% point. Native moves +0.24%. The three-actual interval is wide and
inconclusive; no causal regression, equivalence or useful gain is established.
Allocation stays **169,664B/op**. No confirmation, variant or broader
qualification follows this implementation.

The experiment used validated successive approximation to remove repeated
magnitude-bit and minimum-integer tests in prepared live AC correction. Initial
coefficient storage is zeroed; scan progression validates AH=AL+1 and the prior
band state. Each block/coefficient is visited once, with new insertions behind
the frontier. AC category/approximation bounds keep magnitudes below2^23.
Generic arbitrary-block handling retains all checks. Both modes keep per-bit
polls, input delivery, immediate coefficients and canonical partial cursors.
There is no added field, buffer, owner, cache or session.

One private diagnostic assembly, reused unchanged on Windows/Linux, asserts
**18,764 block entries, 1,182,132 entry values and 91,742 corrections**, including
45,450 negative values. Five bounded inputs cover the frozen packet, high
approximation, restarts, padded420 and small sparse scans. Truncated, malformed
and injected-cancellation executions preserve input/root cleanup. Algebra covers
49,952 signed corrections and546 chain transitions. Already-set-bit and
`Int32.MinValue` counterexamples require the generic entry's full behavior.
This supports the invariant; it is not CPU evidence or general parser equivalence.

The candidate passes **421 internal/375 public JPEG checks per normal/scalar
mode on both OSes**. New independent bitwise-prefix tests cover legal signed
frontiers through low12, near bounds, bit phases, stuffing and truncation; the
existing concurrent-prefix oracle exercises the specialized entry too. Existing
arbitrary-state/overflow controls still pass. One main/embedded candidate build
per OS is reused; worker, fixtures, selectors and native binaries are unchanged.
The initial private-probe directory setup stopped before compilation and was
corrected; no executable came from that setup attempt.

The screen retains the frozen132,961B input and exact531,468B managed/native
RGBA frame. Full public parse/preparation/admission, normalization, independent
output and disposal stay timed. Native contexts and the declared cancellation
asymmetry remain. All original pixel/source/ownership and idle-v2 gates pass.

All production and test sources, Windows/Linux main/embedded modules, consumer
bindings and the Linux source manifest are restored **byte for byte** to ICR614.
The actual private0.2.1 package `80349672` is unchanged. Reuse its **5,938 full/live
checks per OS**, affected scalar/ISA and same-package/three-example proof, without
rebuilds or repeated qualification. Prototype-only entries/tests are removed.
Current benchmark costs stay unchanged; this report is not a renderer dependency.

[Evidence](jpeg-validated-progression-experiment-evidence.json) retains the model,
actual C# observations, candidate, screen and restoration. Mirrored/member-verified
raw `jpeg-validated-progression-experiment.tar.xz`, SHA256
`f68e4fcbb21b644ccc121858d0f08087e9dce529ed02be1b8239933095980739`,
5,580,420B/115 members, preserves reproduction and dependencies. The invariant
may inform a materially different design; it does not authorize a merge or a
repeated microvariant. Next inspect the independent VP8 effort2 preparation graph.
