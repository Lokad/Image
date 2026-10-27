# Completed header acquisition

The shared header reader now returns completed `ValueTask`s directly for
synchronous and successfully completed reads. Small await helpers handle pending
or faulted reads. Counter/limit/cancellation completion stays shared; each pending
read is consumed once and awaited before storage release. The format grammar,
partial reads, acquisition/position, scratch, admission and ownership are unchanged.

Independent JPEG probing confirms **46.5% less elapsed for sync and 50.6% for
async** than the [preceding confirmed build](WARM-PROBE-WORKSPACE.md), with
nonoverlapping filtered/unfiltered confidence bounds. Current ratios are
1.988× / upper2.031 and 2.119× / upper2.154. These remain gaps against the ≤1×
objective. PNG/WebP companion screens are 1.263×/1.289×, without independent
speedup/parity claims. Every control retains **744 B/op** and 512-byte admission.

The [summary](../docs/benchmark.md) and
[evidence](warm-probe-completed-reads-evidence.json) retain current costs, all
actuals, native brackets and quiet guards. The same four controls, frozen packets,
native references and cached benchmark/selectors are reused. No tooling rebuild
or workload grid was added. All twelve preparation/descriptor/ownership/prefix/
coding-scope gates pass.

Both platforms pass 5,040 Release checks each, including 947 live comparisons,
and 98 focused checks each in normal/scalar modes. Three new original controls
alternate immediate and suspended one-byte reads for PNG/JPEG/WebP, with both
stream kinds, All-fact probing and complete header inspection. Exact source
metadata, borrowed stream/root lifetime and released charges agree. Existing
refusal, cancellation, failure cleanup and recovery checks remain passing.
The SAME actual private 0.2.1 package and three usage examples pass both systems;
published releases remain unchanged. One core build per OS serves this round.

Probe/InspectHeaders costs are affected; other-fact and complete-header timings
still need applicable current confirmation. Pixel validation—including its passing
baseline420 control—Identify/Decode input acquisition, packet/pixel kernels and
encoding/editing/text remain unchanged. The mirrored, member-verified raw master
named in the evidence retains sources, actual package, matching Linux binaries,
scripts, logs, guards and receipts; prior immutable tooling remains reusable.
