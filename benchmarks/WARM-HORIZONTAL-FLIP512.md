# Wider RGBA8 horizontal reversal

Single-output RGBA8 horizontal flip improves **22.0%** and independently passes
parity at **0.950× FFmpeg / upper 0.965×**. Two live outputs improve **27.9%**;
that boundary remains a qualified **1.053×** gap. Both retain **128 B/image**.

| Existing boundary | Lokad / FFmpeg µs | Ratio / upper | Managed B/image | Verification |
| --- | ---: | ---: | ---: | --- |
| Two live outputs / per image | 9.56 / 9.08 | 1.053 / 1.142 | 128 | Qualified short gap |
| Single output | 9.08 / 9.55 | 0.950 / 0.965 | 128 | Parity confirmed |

One safe AVX512F permutation reverses 16 complete RGBA8 pixels while retaining
each pixel's channel bytes. The original 256/128/scalar remainder bodies and
entire RGBA16 kernel remain unchanged. The original 1,024-pixel work/cancellation
chunks, row bounds, admission, metadata, pool, failure cleanup and independent
ownership remain. There is no new heap array, table, field, scratch reservation
or logical work charge. Unsupported hardware keeps the original fallbacks.

The original 513×259 RGBA8 raster and complete public flip/acquisition/release
are timed. FFmpeg n8.1 uses its public hflip graph with one filter thread,
prepared graph/source/frame wrappers and fresh non-aliasing pixel output. Native
row alignment is recorded; both expose RGBA rows without extra packing or depth
conversion. Each input/output has 531,468 pixel bytes; compressed size is not
applicable. Native allocation is unmeasured; managed B/image includes harness.

The retained boundary holds both outputs until both flips finish, then releases
both. Generated managed OperationsPerInvoke=2 and every raw native burst counter
scaled by 2, with elapsed unchanged, give per-image time/allocation. The single
boundary has OperationsPerInvoke=1. Exact independent placement, hidden RGB,
source preservation and live-owner independence pass for all three jobs.

Two three-actual 200ms screens use 50ms native pre/post brackets and both 5s idle-v2
guards. One independent twelve-actual single-owner confirmation retains all actuals,
including two not selected by BDN; upper is max(selected/all-actual bound)
against the conservative native lower bracket. CPU0 only, normal SIMD, tiering
off and nonconcurrent workstation GC. The retained mean remains above 1, so no
long repeat follows. One pre-run selector-receipt mismatch failed before any
guard/child/measurement; its original failure is retained. Correcting only that
receipt path enabled the first actual control run; no measurement was repeated.

One immutable producer/OS, frozen harness and compatible archived selectors/
reference/fixtures served the round, with no harness/selector/native rebuild,
new image, workload, settings grid or test. Existing boundary/oracle tests cover
vector tails, work/cancellation chunks, hidden RGB, dirty reused storage, two
live owners, disposal, metadata and failure cleanup.

All **3,739 Release tests per OS** pass: 1,404 internal, 1,437 public, 898 live FFmpeg.
The 67 affected public checks/OS also pass with AVX-512 disabled. Unaffected scalar
qualification is reused. The **same actual private Release package** passes its
consumer and all three usage examples on Windows/Linux. Timed production bytes
remain unchanged through qualification.

[Numeric evidence](warm-horizontal-flip512-evidence.json) retains all 18 actuals,
guards, native raw/normalized counters, source reconciliation and package hashes.
Exactly two selected rows change; 205 comparisons and all 63 prior passing proofs remain.
Full 207-case parity remains open: **64 passes, 142 mean gaps, one confidence case**,
with 123 selected independent confirmations and 84 qualified shorts. The newly
passing single-owner path stops; no vector-width/unroll/inlining grid follows.
