# Bounded JPEG coefficient replay

Existing513×259 JPEG420 controls retain frozen inputs, quality85/native qscale5,
adaptive Huffman and background0x204060. Complete packets remain52,115/52,117 B
for RGBA16 and52,091/52,093 B for RGBA8; only approved attribution COM differs.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| RGBA16 short | 6,637.61 | 1,838.70 | 3.610/4.080 | 372,808 |
| RGBA16 confirm | 6,605.07 | 1,839.29 | 3.591/3.612 | 372,808 |
| RGBA8 current short | 6,339.47 | 1,765.81 | 3.590/3.872 | 372,808 |

Independent RGBA16 confirmation saves33.1% against the
[previous9,874.51-µs confirmation](WARM-JPEG-WORD-NORMALIZATION.md).
Both versioned idle guards pass. Complete operations include full source
normalization, encoding, fresh owned output and release. RGBA8 has one current
cost screen; CPU parity remains open, without broader content/settings claims.

Adaptive420/422 retains quantized coefficients and replays them after building
Huffman tables, avoiding duplicate conversion and transforms. Exact-size storage
is admitted before allocation, capped at4MiB, with simultaneous table workspace.
Tight local allowances select recomputation before admission. Shared refusal
stops the operation; no retry or availability snapshot. Work/cancellation charges
remain equivalent. Fixed-table restarts,444 and grayscale are unchanged.

These controls use430,848 B of coefficient capacity. Warm allocations shown above
reuse the existing bounded pool. Caller-root operations own and charge private
capacity; their new timing/allocation comparison remains open.

Both platforms pass5,052 checks each, including947 live comparisons and148
affected scalar checks. New controls verify cached/recomputed packets and work,
simultaneous admission, refusal/cancellation cleanup and subsequent success.
The same actual private0.2.0 package and all three examples pass on each platform.
Immutable benchmark/native/archived workers are reused.

[Evidence](warm-jpeg-coefficient-replay-evidence.json) records samples, identities,
independent entropy-replay diagnostics, resource costs and qualification.
The larger remaining measured encoder gaps are VP8L and VP8; no further small
JPEG kernel tuning is scheduled before those.
