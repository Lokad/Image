# Local PNG token output

Token emission borrows the existing encoded buffer and holds bit/output state
locally. Literal triplets share one append/drain when their complete bits fit the
existing 32-bit state. Mixed tokens, short tails and wider code groups use the
single-symbol path. The packing guard is arithmetic, not an admission fallback.
No buffer, pool, session or production dependency is added.

Before each actual flush and every exit, state is published to the owner. Output
position is refreshed even when cancellation arrives after a successful write.
Byte order, 4,096-byte publication boundaries, exact packets, cancellation and
shared admission/ownership remain unchanged.

The independent RGBA16 SmallestSize comparison saves **752.47 µs / 3.19% raw /
4.09% reference-normalized** (23,558.06 →22,805.59 µs). Filtered/all-actual raw
and normalized confidence separate: original normalized lower 1.111531 exceeds
selected upper 1.092383. The native clock is **0.929% slower**, disclosed alongside
the raw gain. Byte raw improves 1.09%, but normalized intervals overlap; no byte
incremental gain or regression is claimed.

| Existing complete 513×259 one-shot control | Lokad.Image µs | FFmpeg µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 Smallest9 | 22,805.59 | 21,063.43 | 1.083 / 1.092 | 3,420,448 | 963,202 |
| RGBA8 Smallest9 | 18,021.83 | 17,232.20 | 1.046 / 1.052 | 932,516 | 210,973 |
| RGBA16 Optimal6 | 23,863.19 | 22,409.38 | 1.065 / 1.085 | 3,420,516 | 963,202 |
| RGBA8 Optimal6 | 19,268.37 | 18,681.39 | 1.031 / 1.047 | 932,516 | 210,973 |
| RGBA16 Fastest1 | 21,990.88 | 20,137.44 | 1.092 / 1.111 | 3,420,516 | 968,216 |
| RGBA8 Fastest1 | 5,939.86 | 4,718.00 | 1.259 / 1.284 | 951,212 | 236,828 |

All six retain parity gaps. Four already-required level-1/6 boundaries are
refreshed on the same immutable build without new inputs/settings or a workload
grid. Their native clocks are about **8–9% slower** than earlier observations;
raw managed costs are also higher. These are current costs, not raw incremental
speedup or causal regression claims. Complete fresh codec/zlib/packet state and
independent output remain timed on both sides; the native word frame’s allocation,
endian conversion and disposal also stay timed. Twelve actuals, both raw estimators,
all native brackets and before/after quiet guards are retained.

Allocation/storage are unchanged. Word Smallest batches nine complete operations
per actual versus the original eight; its 6 B/op difference is harness amortization.
Byte and the four other confirmations batch four. Deflate admission remains
**384 KiB**, retained operation scratch zero; no newly measured complete admitted/
physical peak or native allocation figure is inferred.

The cursor-only stage’s word screen is noisy and inconclusive; its two short
results remain in the evidence. The revised stage removes repeated literal token/
append/drain dispatch, with no repeat of the original stage for favorable points.
A prior count audit rules out useful wider filtering on the word control: only
67 candidates, 176 scalar comparisons and no word calls are avoidable. Counts are
not a CPU savings bound; the diagnostic/raw dependency is retained.

Selected production passes **5,566 full/live checks per OS** (2,343 internal,
2,275 public, 948 differential), plus **573 internal/211 public PNG checks per
normal/scalar mode and OS**. Nine real-stream controls inject failure before a
write, after partial acceptance, and cancellation after a full write at three
boundaries; they verify frozen prefixes, charged users, no cleanup writes and
subsequent root reuse. An independent scalar canonical-bit oracle checks the
actual token writer in 576 valid-tree/alignment/run/buffer-edge scenarios. Complete
packet/decompression tests cover real inputs and other compression levels.

The SAME actual Release package and all three examples pass Windows/Linux.
Published releases are unchanged; this is development 0.2.1, not a release refresh
or publication. Two immutable core/internal versions per OS cover the coherent
experiment and qualification; no harness/native/probe/selector/public builds,
one package. Only level-1/6/9 token-output costs are invalidated. Other affected
colors/depths/async costs remain historical; unchanged paths reuse their proof.
The full 282-path objective stays open.

[Evidence](png-token-output-evidence.json) pins exact sources, both stages, six
current costs, original baselines, confidence/clock differences, all gates and
actual-package qualification. Mirrored/member-verified raw `png-token-output-qualified.tar.xz`: 5,618,852 B / 298 members,
SHA256 `1124e116725df5d658d2c4262d929b24f22e82e16a77c1e6f065556dbcca9c60`. Producer/test snapshots, package/symbols, guards,
actuals, failure/oracle checks and reproduction are retained; baseline, reference,
current-source attribution and count-audit archives are pinned dependencies.
Earlier [dictionary preparation](PNG-SEQUENTIAL-DICTIONARY-HASH.md),
[greedy state](PNG-FAST-LOCAL-EXECUTION-STATE.md),
[one-shot costs](PNG-SMALLEST-ONE-SHOT-CURRENT.md) and
[search execution stages](PNG-SEARCH-EXECUTION-INCONCLUSIVE.md) remain history.
