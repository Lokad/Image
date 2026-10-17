# Cancelable JPEG444 entropy traversal

Historical ≤2x checkpoint. Its live-token baseline444 timing is superseded by
[bounded AC cache reuse](WARM-JPEG-CANCELABLE-AC-CACHE.md), a current qualified
short gap under the ≤1x objective. Other selected paths retain their applicable
evidence in the [current summary](../docs/benchmark.md).

Complete warm decoding with a live, never-signaled token improves **26.0%**, from
**2.383×** FFmpeg to independently confirmed **1.767× / upper 1.780×**, at
**3,046.63 µs** and **2,664,373 managed B/op**. All twelve actuals are retained,
including one BDN-filtered outlier; the bound includes filtered/all-actual
uncertainty against the faster native bracket. Every versioned quiet guard passes.

| Stage / fixed workload | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Original screen | 4,116.71 | 1,727.21 | 2.383 / 2.509 | 2,664,332 |
| Accepted screen | 3,032.81 | 1,723.70 | 1.759 / 1.830 | 2,664,373 |
| Independent confirmation | 3,046.63 | 1,723.88 | 1.767 / 1.780 | 2,664,373 |
| Ordinary native-input JPEG444 regression screen | 3,357.60 | 1,719.85 | 1.952 / 2.216 | 2,664,358 |
| Progressive JPEG444 regression screen | 6,341.69 | 3,328.96 | 1.905 / 2.188 | 2,682,364 |

The original 513×259 byte raster and native 133,041-byte JPEG444 packet remain
frozen. The boundary includes public span decoding, full-range BT.601 conversion,
independent owned output acquisition/release and disposal on both subjects.
Token/source setup, startup, I/O, IPC, fixtures and validation are excluded.
Native has no equivalent cancellation setting; its codec/conversion contexts and
packet/frame wrappers are reused, with complete state reset and pixel work each
operation. No output is cached. Native allocation totals are unmeasured.
Managed allocation figures include diagnostic/harness overhead; no new per-image
heap workspace is introduced.

One six-second profile of an early candidate attributes about 71% to entropy/block
traversal. The
accepted primitive keeps consecutive ordinary AC coefficients' bit cache,
position, coefficient index and exact block work budget local. Safe two-byte
lookahead returns unused whole bytes before publishing the original pending bits
and reader position. Prefix/amplitude retain separate cancellation checkpoints
and work admissions; completed work is committed on every exit without another
cancellation check. Stuffing, markers, long codes, invalid syntax and constrained
cases retain original traversal and partial failure state. No new table, unsafe
code, dependency or upstream material is added.

Exact managed pixels, opaque alpha, geometry, source and owner independence are
unchanged. Existing normal/scalar native maximum RGB4 / RMSE1.5 bounds remain.
An independent scalar block oracle compares every budget/truncation, predictor,
partial block, exception and reader state. Six new checks include all 65,536 raw
byte pairs, cached phases, signed amplitude sizes, long-prefix fallback, restart
and canceled-scope completed charges. Pre-canceled public calls retain token identity.

Local accounting alone, forced inlining and wider existing lookups did not yield
useful wins and remain rejected. An earlier two-byte candidate's independent mean
exceeded two; the first burst version passed its mean but failed its conservative
bound. A launcher attempt with twelve managed/three native samples failed metadata
validation and is unscored. All attempts and actuals remain retained. The final
coefficient-local loop removes repeated reference loads/stores and moves failure
state calculations off the successful path. The same frozen production/harness
serves the accepted screen, confirmation and both regression screens; generated
target drivers and the native worker are reused. Regression means were read before
full qualification; their short bounds do not replace prior independent certificates.
Only three workload types and the two existing original rasters are used.

All **3,349 Release tests per OS**, including **896 live FFmpeg comparisons**, and
**458 focused scalar checks per OS** pass. The same actual dependency-free Release
package passes its consumer and all three examples on Windows and Linux. Runs
are sequential Release on one pinned CPU, one codec/conversion thread, normal SIMD,
tiering off and workstation nonconcurrent GC. Measured production/benchmark/test
inputs remain unchanged through qualification; only SPEC/resource documentation
changes after measurement.

[Numeric evidence](warm-jpeg444canceldecode-evidence.json) retains exact source,
jobs, allocations, guards, all actuals, rejected trials, profile and package proof.
Stop this confirmed fixed pair; full material/current-source warm coverage remains open.
