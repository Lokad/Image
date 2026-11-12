# Wider Adler trial

The 64-byte AVX512 Adler implementation is **not retained**. Qualified production
source, Windows/Linux core and embedded-test modules are restored byte-for-byte;
the actual development package is unchanged. Current PNG costs remain owned by
[wide chunk CRC](PNG-WIDE-CHUNK-CRC.md) and [block Adler](PNG-BLOCK-ADLER.md).

| Complete quiet-VM operation | Contemporary baseline → trial µs | Result |
| --- | ---: | --- |
| Stored RGBA16, retained native, post-GC4 | 474.63 → 435.43 | 39.20 µs / 8.26% raw gain; normalized confidence overlaps |
| Stored RGBA8, same boundary | 69.52 → 62.51 | Filtered raw gain; all-actual and normalized confidence overlap |
| Materialized RGBA16 decode | 2,812.11 → 2,790.58 | 21.53 µs / 0.77% raw gain; normalized confidence overlaps |
| SmallestSize RGBA16, fresh codec state | 20,268.31 → 21,838.92 | 1,570.62 µs / 7.75% raw adverse; normalized adverse also separates |

Filtered and all-actual raw intervals separate for the word encode, word decode
and compressed adverse result. Native means move −6.42%, +0.09% and +5.11%
respectively. SmallestSize's normalized adverse point is 2.51%, with separated
bounds. The compressed concern outweighs the scoped stored benefit. These are
scoped observations, not a uniform causal regression or a ban on wider SIMD.
There is no further sampling to seek favorable results.

The original three-actual stored-word screen precedes independent 12-actual
confirmations. The compressed concern first appears against historical evidence;
one contemporary unchanged-baseline confirmation resolves it. All actuals,
native brackets, original settings, quiet CPU0 guards and complete ownership
boundaries remain. Retained-native and fresh-state comparisons stay distinct.
Twenty encoder views preserve exact complete packets; eleven read/validation
views preserve their original exact-output gates. The batch stops after a fully
recorded capture, before the remaining 40 reads, shared-root and natural-GC views.
This incomplete trial is not accepted evidence for those paths.

The implementation applies the same bounded recurrence to 64-byte groups, then
uses the existing 32-byte and scalar tails. An independent coefficient proof checks
133,120 positions and intermediate overflow bounds. Windows/Linux each pass 704
internal/246 public focused checks in normal, scalar, AVX512-off and AVX2-off
modes. No storage, ownership, cancellation, modulo or output contract changes;
captured allocation counts remain unchanged. Physical/native/JIT peaks are
unmeasured. No new tests or production code are retained, so the restored producer's
6,512 full/live checks and SAME-package consumers on both systems are reused.
There is no additional broad qualification, rebuild or package proof.

A preceding read-only actual-package profile supplies the hypothesis: 836 user
locations, 337 mapped managed; CRC chunk/finalization accounts for 142/18 and
Adler for 94. The other 499 locations remain unassigned. Samples, inlining and
different profile batching do not establish component times or removable costs.
Its raw archive is separately pinned in the evidence.

[Evidence](png-wide-adler-trial-evidence.json) retains both sources/modules,
algebra, harnesses, captures, preflight failures and restoration proof. Raw
`png-wide-adler-trial.tar.xz`, SHA256
`23cb37fb4d456c58ac38eab764bfaae4ae66b710623051f23951ea824d470178`
(1,763,684 B / 495 members), is mirrored and member-verified. Published releases
are untouched. Continue an independent justified task; do not replay this
implementation or replace current summaries with its trial costs.
