# PNG prepared-token investigation

Two exact implementations remove duplicate copy-symbol classification between
block frequency collection and token output. Neither establishes a useful gain;
both are restored. Current production, package and displayed costs are unchanged.

The first prepares RFC1951 symbols/residuals in the existing uint token slots and
returns symbols separately for frequency collection. Generated-code inspection
shows the by-reference handoff; the second returns one packed value and derives
frequencies from it. This is a coherent representation revision, not a timing
repeat. It adds no array, pool, cache, reservation or retained capacity. Stored
blocks may prepare fields they never emit. Neither stage changes token order,
ties, block selection, output flushing, cancellation or failure cleanup.

| Complete frozen 513×259 encode | Original short µs / ratio | First stage µs / ratio | Single-value stage µs / ratio | Managed B/op, all three | Exact packet B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 Fastest1 | 5,673.313 / 1.303 | 5,840.815 / 1.310 | 5,746.221 / 1.323 | 820,849 | 236,828 |
| RGBA16 Smallest9 | 22,436.656 / 1.072 | 22,662.283 / 1.087 | 22,565.041 / 1.083 | 2,437,584 | 963,202 |

Quiet-VM CPU0, fresh one-shot codecs/zlib/output, complete encode and independent
output ownership on both sides; frozen source/settings and all original packet,
pixel, ownership and guard gates pass. Each short job has three actuals; native
allocations are unmeasured. These are screens, not independent confirmation or
new parity proof. All filtered/unfiltered raw and normalized confidence views
overlap. In particular, the original byte screen has a wide upper ratio of2.840.

Initially reuse the valid twelve-actual block-preparation baselines: their paths
are unchanged. Apparent raw improvements disagree with normalized results because
the native reference is6–8% faster. One fresh original short job per control
resolves the missing comparison; all candidate actuals are reused. Against those
fresh originals, neither stage has a favorable raw or normalized point estimate,
and allocations are identical. This does not establish a causal regression.
No candidate repeat or independent confirmation follows. The original observer
prefix is corrected during postprocessing without repeating completed work.

One untimed generated-code check reuses six existing packet cases. The first
stage has a267B preparation helper and2,039B writer, versus the original2,215B
writer. Code size/calls motivate revision; they do not identify elapsed savings.
Both stages pass581internal/211public Linux checks per normal/scalar mode.
Only Linux candidate modules are built; saved Windows candidate sources are
explicitly source-only. Originals are restored byte-for-byte without rebuilding.

One useful test remains against the **actual original writer**: all256 copy
lengths crossed with56 independently derived distance-range endpoints, including
32,768, produce14,336 tokens checked against an independent canonical/extra-bit
oracle. The existing576 alignment/flush combinations remain. Both writer facts
pass normal/scalar on Windows/Linux after one internal-test-only build per OS.
Reuse unchanged WCP2815,628full/live checks perOS and the same actual0.2.1package
(`b9e43cf9…`)/all-three-example verification; do not infer a fresh5,629-case run.

[Evidence](png-prepared-tokens-inconclusive-evidence.json) pins both candidate
sources/modules, all actuals/brackets, guards, restoration and retained checks.
Mirrored/member-verified raw `png-prepared-tokens-inconclusive.tar.xz`:
3,069,824B/289members, SHA256
`2824e489704ada5a70a2b5050c0fb0608e7119b057717593afc46cf77e7a0ec4`.
Qualified dependencies and the motivating [current attribution](PNG-FAST8-CURRENT-ATTRIBUTION.md)
remain linked. This implementation is inconclusive, not a permanent family ban.
The full282-path objective stays open; publication is postponed.
