# Bounded-row copy traversal trial

**Inconclusive, unaccepted:** complete original RGBA16 overlap pairs measure
**5.656552 → 5.685088 µs**, ratio interval **0.989047–1.021300**. Allocation is
**0 B/op**. The three reversed alternating 16,384-operation pairs show neither
a useful gain nor a causal regression. Stop without confirmation, neighboring
variants, standalone-reference retiming or production promotion.

The isolated prototype bypasses inner chunk/count/offset bookkeeping when a
clipped row fits the existing 4,096-pixel cancellation chunk. It preserves the
wide-copy body, row advance, snapshot traversal, bulk/tail kernels, validation,
admission and the same logical cancellation checkpoints. No buffer, cache, owner
or API change. Generated code realizes the branch, growing the caller from the
source-valid 1,364 B to 1,726 B; this does not explain the inconclusive timing.

Both OSes pass **190 existing public editing/ownership checks per normal/scalar
mode**. Both workers pass original full-word snapshots, hidden RGB, borrowed-input
integrity, cancellation/refusal cleanup and recovery. The frozen 513×259 canvas,
257×129 rectangle and complete forward/reverse public calls remain unchanged.
Persistent default-loader workers, CPU0, natural nonconcurrent GC, 16,384 warmups,
two clocks per batch and versioned quiet guards are preserved. Every actual is
retained. This DLL pair is not an external-reference performance comparison.

One isolated Windows core build; zero VM, canonical, embedded/public-test,
harness/reference/fixture/package builds. Canonical sources and modules remain
unchanged. Reuse 6,776 full/live checks and the same actual Release-package
consumers/three examples each OS. Added live/retained capacity is zero;
physical/native/JIT peaks are unmeasured.

[Evidence](copy-bounded-traversal-trial-evidence.json) pins implementations,
modules, all actuals, guards and reused qualification. Mirrored/member-verified
raw `copy-bounded-traversal-trial.tar.xz`: **2,043,508 B / 52 members**, SHA256
`80d32167dc46d9eafee9c6f87442e8e2e97c436a835d917ab61867c21c5bde59`.
Archived `bct1169-*` recipes and declared dependencies reproduce the round.
Current costs do not change; this rejected experiment is not a renderer input.
The finite 289-path objective remains open and publication stays postponed.
