# Exact VP8L color-trial reuse

The fixed RGBA8 control independently measures **181,640.59 µs, 1.486× reference
time / upper1.507**, 6.3% below the unchanged producer's 193,847.00 µs confirmation.
These are separate quiet confirmations, without a paired-baseline confidence
claim. Full warm CPU parity remains open.

All controls retain 513×259 inputs, Q75/effort4, libwebp exact=1 (FFmpeg's backend),
exact complete packets and single-CPU complete-operation boundaries. Normalization,
fresh independently owned output, async staging/delivery and release stay timed.

| Existing control | Lokad µs | Ratio / upper | Allocated B/op | Both packet B |
| --- | ---: | ---: | ---: | ---: |
| RGBA8, independent 12 actuals | 181,640.59 | 1.486 / 1.507 | 29,688,704 | 183,812 |
| RGBA16, short 3 actuals | 181,852.57 | 1.481 / 1.755 | 29,688,704 | 183,812 |
| Async nonseekable, short 3 actuals | 183,748.63 | 1.515 / 2.118 | 29,689,088 | 183,812 |
| Photograph, short 3 actuals | 131,449.38 | 1.583 / 2.079 | 20,409,400 | 160,032 |

Source tile, history and neighbor preferences remain fixed through each color
phase. Exact repeated scores reuse the opposite plane's unused products; blue
products are prepared after red search, and blue trials reuse finished red storage.
Each phase/tile resets the cache. No array capacity or admission attempt is added.
The owner counter adds 8 allocated B/op within the existing header allowance;
the admitted 7,296 B scratch peak stays exact. Cancellation precedes lookup.

Both OS pass 4,959 Release checks (1,977 internal, 2,035 public, 947 live), ten
focused scalar checks and eight own-source map/residual/capacity comparisons.
The SAME actual private 0.2.1 package and all three usage examples pass both OS.
One production build per OS and cached harnesses/workers/selectors are reused.
All actuals, exact source/packet/ownership checks and idle-v2 guards pass.

Only color-search encoding costs change, including applicable normalization,
async and method5/6 callers. Diagram4 skips prediction/color search; compressed
VP8 alpha skips color search. Their valid costs, unrelated codecs/decoders/editing
and native proofs are reused. Four displayed callers are refreshed; other affected
inventory paths need current timing evidence before a CPU-pass claim.
[Numeric evidence](warm-vp8l-color-trial-reuse-evidence.json) identifies producers,
the actual package, reference reuse and mirrored member-verified raw master.
