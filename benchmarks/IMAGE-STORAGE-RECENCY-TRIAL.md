# Image storage recency trial

**Inconclusive, unaccepted.** The isolated recency change realizes one-backing
reuse, but complete original word flips measure **17.718 → 17.561 µs**:
0.157 µs at the point estimate, ratio interval **0.956154–1.027436**.
The three reversed alternating 16,384-operation pairs establish no useful gain
or causal regression. Allocation remains **56 B/op**, retained capacity unchanged.
Stop without independent confirmation, byte/reference retiming or promotion.

The [storage audit](OWNED-FLIP-STORAGE-BOUNDARY.md) motivates this architectural
experiment. `ReturnLargest` publishes recently returned image storage first while
keeping retention eligibility, largest replacement, duplicate protection, slot/
byte ceilings and the admitted retained set. Ordinary `Return`, `ReturnLatest`,
caller-root bypass, pixels, kernels and owner lifetimes remain unchanged. No new
pool, cache, API or storage. Actual byte/word probes on both OSes change recurring
identities from **1,0,1,0,1,0,1,0** to **0,0,0,0,0,0,0,0**, preserving exact original
frames and independent live results. This establishes no physical/cache benefit.

Windows passes **30 existing pool tests compiled with the changed source**;
Windows/Linux each pass **190 public editing/ownership checks per normal/scalar
mode**. Existing slot/byte ceilings, mixed sizes, independent leases, concurrent
use, clearing, cancellation, refusal and recovery pass. Public runner bindings
and canonical modules are restored byte-for-byte with original timestamps.

The paired screen reuses the original frozen public `WarmVerticalFlip.Managed`
operation and stock reference checks. Public result creation, materialization
and disposal remain inside every call; hashing, setup, IPC and full-frame checks
stay outside. Two persistent default-loader workers warm 16,384 operations each,
then alternate complete batches on CPU0 with natural nonconcurrent GC, two batch
clocks and passing versioned quiet guards. Native setup/cleanup brackets supply
correctness checks only; their clocks are unused. This is an incremental DLL
comparison, not standalone FFmpeg parity.

A clone-prefix error initially omitted the frozen benchmark DLL/deps, exiting
before CLR/setup or samples. After linking those files, all actuals, cleanup and
guards completed. The recorder then incorrectly expected three smoke brackets;
offline recovery requires the exact original receipt's counts and all unchanged
gates. Every actual and failed preflight remains; zero operations repeat.

One isolated core, one focused pool-source test assembly and one small actor were
built on Windows; zero VM, canonical, embedded/public-test, existing harness,
baseline, fixture or package builds. Unchanged production reuses 6,776 full/live
checks and the same actual package/three examples per OS. Added live/retained
capacity is zero; physical/native/JIT peaks are unmeasured.

[Evidence](image-storage-recency-trial-evidence.json) pins implementations, all
actuals, recovery, guards and reused qualification. Mirrored/member-verified raw
`image-storage-recency-trial.tar.xz`: **1,621,192 B / 75 members**, SHA256
`6c37a96b065f19079fdae37e84d9f18672ff46c5f499a0eee7d58d980b41e69f`.
Archived `mrr1171-*` recipes and dependencies reproduce the round. Keep qualified
FIFO image retention and current uncertain costs. No neighboring recency/slot
variants follow this screen; this report is not a renderer dependency.
