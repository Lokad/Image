# Reuse all four caller-CMYK coefficient buffers

The existing caller-CMYK complete decode uses **7.5% less elapsed time** and
**549,351 fewer allocated bytes per operation (33.6%)**, preserving exact pixels.
Its **1.664× / upper1.672** FFmpeg cost remains a gap.

| Complete 513×259 decode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prior DC-only caller CMYK, independent | 600.31 | 332.30 | 1.807 / 1.817 | 1,634,913 | 10,895 |
| Four-buffer caller CMYK, independent | 555.26 | 333.64 | 1.664 / 1.672 | 1,085,562 | 10,895 |
| Current sparse-CMYK, owned RGBA8 | 733.93 | 437.75 | 1.677 / 1.688 | 1,085,520 | 22,708 |
| Baseline420, unchanged-path evidence | 1,124.01 | 1,008.25 | 1.115 / 1.120 | 757,328 | 52,093 |

These controls use default limits without a supplied shared memory budget in
separate warm processes. The caller-only process previously retained three
coefficient arrays despite needing four simultaneously. The existing owned-CMYK
four-slot policy now runs at the shared four-component materialization boundary.
All four arrays fit the unchanged **4 MiB idle-payload ceiling**. This control
may retain **549,120 more payload bytes**, avoiding the fourth allocation each
operation. Exact lengths, zero-before-reuse, eviction, duplicate protection,
nonblocking contention and independent active leases stay unchanged. No new
cache, budget or ownership mechanism is introduced.

**Shared-budget operations still bypass the pool.** They reserve before allocation
and release after actual users as before. Caller samples remain borrowed and
independent; only private coefficient arrays enter this cache. No mixed-history,
concurrency, other-content or governed-path performance claim follows.

Owned sparse-CMYK is an affected current-cost companion, without a fresh paired
causal gain claim. Its measured allocation is unchanged. The ordinary fresh-process
three-component420 path never calls the changed methods; source-route hashes
justify reusing its valid prior twelve. Native allocations are unmeasured.
Original packets, input delivery, settings, normalization, complete output
acquisition/release, exact normal/scalar/native pixels and owners remain frozen.

A fresh three-actual pair clears the strict 5% raw/normalized screen. One candidate
twelve reuses the valid original twelve: **7.504% filtered / 7.469% unfiltered
elapsed**, **7.878% normalized-point** improvement. Raw 99.9% CIs and conservative
normalized envelopes separately separate. The allocation gain does not substitute
for the CPU gate. One owned sparse-CMYK twelve and all idle-v2/native/ownership
gates pass. The wide original three-sample interval is retained; no job is repeated.

All **5,171 checks/OS** pass (2,088 internal, 2,135 public, 948 live), plus
38 internal/98 public focused checks normal/scalar per OS. Existing pool/growth
tests cover cap, zeroing, identities, eviction, disabled storage and concurrency;
existing public tests cover caller/root/refusal/cancellation/cleanup. No new tests
merely mirror the moved call. The same actual Release package, governed streaming
consumer and three examples pass on both platforms. One core build/OS; zero
harness/native/selector builds.

Stop this policy primitive. The two CMYK controls around 1.67× remain the largest
confirmed material gaps; their small ordering difference does not justify another
timing round. Other affected four-component boundaries need applicable costs
before CPU-pass claims. The full **268-path ≤1×** objective remains open.

[Evidence](warm-jpeg-caller-coefficient-slots-evidence.json),
[prior zero-row work](WARM-JPEG-ZERO-UPPER-ROWS.md),
[existing owned reuse policy](WARM-JPEG-COEFFICIENT-REUSE.md).
Mirrored/member-verified jpeg-caller-coefficient-slots-qualified.tar.xz, 159 members,
SHA256 c0e6be623ceec47bb0ecb0dbeea8a53cd6e48f4f33642642a294efc7a58adc2c; dependencies are pinned by hash.
