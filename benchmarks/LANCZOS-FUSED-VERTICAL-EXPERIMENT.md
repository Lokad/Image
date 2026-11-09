# Ordered fused vertical traversal

Neither implementation is selected. LFV691 prepares the needed horizontal rows
before vertical accumulation, then keeps each sample's ordered FMA chain in
registers. LFC692 revises it to prepare exact double coefficients once per output
row. Both preserve Q30 coefficients, final certification/rounding/repair and
independent output ownership. Larger windows retain streaming execution.

| Complete prepared control | Original → candidate µs | Native original → candidate µs | Allocated B/op |
| --- | ---: | ---: | ---: |
| LFV691 RGBA16 enlargement | 4,408.62 → 4,680.63 | 4,228.94 → 4,252.68 | 4,265,302 |
| LFV691 RGBA16 reduction | 596.97 → 635.07 | 556.42 → 530.64 | 139 |
| LFC692 RGBA16 enlargement | 4,401.39 → 4,183.71 | 4,199.60 → 3,908.93 | 4,265,302 |
| LFC692 RGBA16 reduction | 550.56 → 631.99 | 517.03 → 551.55 | 139 |

Every job has three actuals, native brackets, quiet guards and all frozen exact
managed/native/source/ownership gates. Full source reads, preparation, row reset,
independent output and disposal remain timed. Native prepared graph/input/scratch
remain retained and labeled. Allocation and retained plan capacity are unchanged;
the revision adds only 128 bytes of minor stack coefficient preparation.

The first implementation has adverse complete points on both controls. The
revision's enlargement raw point improves while its native-normalized point
worsens; reduction points remain adverse. Broad short confidence establishes no
causal gain, regression or equivalence. Neither design supplies a useful complete
lead for independent confirmation. No favorable repeat or further variant follows
this round. These outcomes do not ban materially revised staging designs.

Both variants pass 251 internal / 729 public focused checks per normal/scalar
mode on each OS. Production sources, main modules and public bindings are
restored byte for byte. The actual qualified Release package is unchanged.
Unchanged full Windows/Linux/live/scalar/ISA/package evidence is reused, without
a production rebuild or repeated broad/package checks.

Six meaningful byte/word cache-boundary cases remain: ordinary short windows,
eligible edges with longer interiors, and support exceeding the 64-slot cache.
They compare complete results against actual integer execution and verify
independent retained ownership. All six pass normal/scalar/AVX-512-disabled on
both OSes after restoration; only embedded test assemblies rebuild. Tests solely
for the discarded helper are removed.

[Evidence](lanczos-fused-vertical-evidence.json) preserves both designs, every
actual and exact restoration. Mirrored/member-verified raw
`lanczos-fused-vertical-experiment.tar.xz` has25,988,384B/192members, SHA256
`ea941aa22591f7aed86c9b33346cad32132b65bf909549724109c2e7624c4b16`.
It adds no renderer dependency; [qualified current costs](LANCZOS-WORD-CERTIFIED-ROWS.md)
remain unchanged. A different lead is using preparation staging to borrow the
sum row for exact source-word conversion, then retaining the existing vertical
kernel. This requires its own lifetime proof and complete measurements.
The subsequent [source-conversion experiment](LANCZOS-SOURCE-CONVERSION-EXPERIMENT.md)
records that different implementation and its restoration. Compression correctness
and the full performance objective remain; publication is postponed.
