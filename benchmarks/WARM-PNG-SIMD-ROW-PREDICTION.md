# Exact SIMD PNG row prediction

The existing predictable 3000² RGBA16 horizontal-flip control keeps default Optimal,
pinned FFmpeg n8.1 level6/pred=mixed and its complete 573,041-byte packet, SHA256
`4b1794a8886a95c14a899be3e15418bf198e22fc0e59a3af7d15c33df4c8e205`.
Every sample remains exact; the <2 MiB practical-compression gate is unchanged.

| Quiet CPU0 observation | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Candidate short | 693,687.02 | 1.697 / 2.023 | 3,402,944 |
| Fresh baseline confirmation | 1,105,979.70 | 2.795 / 2.958 | 3,402,944 |
| Candidate confirmation | 686,596.68 | 1.691 / 1.727 | 3,402,944 |

Fresh independent confirmation finds **37.92% less filtered elapsed**, 38.12%
across all actuals. Intervals separate in both forms. Allocation is unchanged;
no owned capacity is added. Twelve actuals, both native brackets, exact packets/
pixels, source/independent-output checks and idle-v2 guards pass. The remaining
1.691× / upper1.727 CPU gap is open; this is not full parity.

AVX2 scores 32 wrapped residual bytes together and computes average/Paeth without
per-byte branches. Strict ties and residual bytes match the original scalar body,
which remains the portable fallback. Average corrects the intrinsic's upward
rounding; Paeth distances fit signed16 (maximum510). Selected samples remain
0..255. SAD sums fit signed64 for admitted array lengths. Loads stay within
packed spans; prefixes/tails and direct cancellation checkpoints remain bounded.
No tables, buffers, caches, admission changes, dependencies or unsafe code are added.

Before production edits, both OSes pass an independent authored 324-row policy
digest plus exact unfilter reconstruction: six legal strides, all five filters,
ties, signed-byte magnitudes, unaligned spans, vector/tail/4 KiB edges and guards.
The digest is `b005b3dc378748340fef2832955096685609db246043e075c15005f9501d611f`.
Candidate normal/scalar checks retain it. Six large opaque geometries stay small
and lossless; 8/16-bit transparency, hidden RGB, sync/async, terminal shared refusal,
cancellation/refund/recovery, streams and the fourteen prior packets pass.

Each OS passes 5,065 full checks (1,994 internal, 2,123 public, 948 live), plus
604 internal and 217 public PNG/Zlib checks with intrinsics disabled. The same
actual Release package and all three usage examples pass on both platforms,
including the packaged independent PNG regression. Published packages stay immutable.
One production build/OS is reused through the bounded round; harness/selectors/
native workers are cached. No new workload grid or whole-library audit is added.

Only selected predictive rows change. Sampling, the four displayed None/stored
encoder paths, decoding/inspection, other codecs and editing/text retain applicable
evidence. Other affected predictive-input CPU costs need current evidence before
pass claims. The [prior attribution](PREDICTIVE-PNG-CURRENT.md) keeps Deflate
search as a separate substantial lead; its code and packet/token decisions are
unchanged by this primitive.

[Machine-readable results](warm-png-simd-row-prediction-evidence.json) identify
producers, confidence, allocation and qualification. Mirrored/member-verified raw
`png-simd-row-prediction-qualified.tar.xz` (127 members), SHA256
`9a2a6943eaf9b840876f7fb922f5b8904936770542d2b911dc84238c885001e0`,
retains baseline/candidate producers, source patch, original row oracle,
pre-adoption/full/scalar/package logs, actual packages, guards and receipts.
