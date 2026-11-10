# Lanczos byte-row experiments

Neither byte-row implementation is selected. The original production DLLs,
certificate, source and consumer bindings are restored exactly. The current
prepared RGBA8 reduction remains a gap: **580.52 / prepared zimg 489.96 µs**,
conservative upper **1.195**, GC **139 B/op**, retained plan **139,800 B**.
This contemporary twelve-actual confirmation replaces the older 625.69/532.52 µs
cost. Clock movement prevents a causal historical speed or allocation claim.

| Existing complete prepared reduction | Managed µs | Native µs | Evidence / decision |
| --- | ---: | ---: | --- |
| Original production | 580.52 | 489.96 | 12 actuals, 11 filtered; gap |
| Direct exact byte FMA rows | 604.43 | 491.11 | 3-actual screen, wide intervals; no useful lead |
| Integer rows with direct double stores | 602.48 | 490.93 | Independent 12-actual confirmation; reject |

The direct-store revision is 3.78% raw / 3.58% normalized slower by point
estimate, with native +0.198%. Raw loss separates in filtered and all-actual
views; normalized intervals overlap, so no confirmed normalized regression is
claimed. Its initial short 611.37/native527.28 µs was inconclusive. One original
confirmation resolved reference-clock movement before the final candidate run;
there are no additional favorable repeats.

LBX741 proves byte alpha products and Q30 partial sums fit exact binary64 integer
arithmetic (<2^47 RGB, <2^39 alpha). LBX742 uses independent exact FMA chains;
LDS743 retains the original integer arithmetic and changes only final stores.
Both remove the whole-row conversion pass. The complete original integer kernel
already batches four pixels with AVX512; the FMA prototype's two AVX2 pair
unpacks add work. This corrects the initial incomplete source reading. Actual JIT
and source undo proofs retain both prototypes, including unchanged word kernels.
These are implementation results, not permanent bans on a revised design.

A subsequent **two-second sampling profile** of unchanged current production
collects 1,998 samples: horizontal integer filtering 1,140 (57.1%), vertical
accumulation 492 (24.6%), writer 232 (11.6%), and the wrapper containing row
conversion 60 (3.0%). This supports investigating shared horizontal preparation,
not another conversion-only rewrite. Sampling proportions are approximate,
not exact phase costs or measured prospective gains. Instrumented elapsed times
are discarded. No new benchmark images, settings, harnesses or native workers.

Each prototype passes 327 internal / 729 public resize-focused cases in normal
and scalar modes on Windows/Linux. After exact restoration, the retained five
new byte-row cases and stronger BigInteger bit oracle pass the affected 19-case
class in normal/scalar/AVX2-off/AVX512-off modes on both OSes. They check exact
rows, tails, long support, transparent samples, guards, unchanged input, zero
producer allocation and shared-root retention/release against real production.
ISA-specific kernel bodies run only when their required instructions are present;
the unchanged portable fallback retains its existing qualification.
Only embedded tests rebuild during restoration. Unchanged **6,148 full/live checks
per OS** and SAME actual package/all three examples are reused, not rerun.

Both timed sides preserve prepared state, source reads, exact execution/native
normalization, independent output and disposal. Original native pixel gates,
single-CPU/idle-v2 guards and ownership checks pass. No storage capacity, pool,
numerical policy, cancellation or admission change survives. Plan retention is
separate from output ownership; physical/native peaks remain unmeasured.

[Current cost](lanczos-byte-current-cost-evidence.json) supplies the displayed
result. [Study evidence](lanczos-byte-row-experiment-evidence.json) retains all
adverse screens, confirmations, code proofs and restoration. The renderer does
not depend on rejected-prototype checks. Raw archive
`tmp/lanczos-byte-row-experiment.tar.xz`, SHA-256
`f0640cfc95998fca027aa4e13ab4d6dd9528ba644ea135d76b5db19252b551b8`,
19,576,284 bytes / 279 members, is mirrored and member-verified. Its dependency
is the [qualified paired-word round](LANCZOS-PAIRED-WORD-WINDOWS.md).
