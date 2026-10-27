# Exact indexed VP8L histogram reconstruction

The VP8-plus-alpha cost row is superseded by [coefficient-domain validation](WARM-VP8-SIGNED-DOMAIN-GUARD.md); the photograph short now has an [independent confirmation](WARM-VP8L-PHOTOGRAPH-CONFIRMATION.md). Other VP8L-only costs and the ownership proof remain valid.

The existing async control independently measures 197.78 ms, 1.622× reference
(upper 1.641×), exact 183,812 B and 29,689,080 B/op. Allocation falls 37.73% from
47,675,312 B/op; elapsed is 2.13% below the preceding independent mean. Diagram
independently measures 11.12 ms, 1.166×/upper 1.204, exact 7,234 B and 27,419,167 B/op:
15.68% less elapsed than its preceding independent mean. Separate runs are not
paired baseline gains. Async retains 12 actual/filtered; diagram 12 actual/11 filtered.
Conservative bounds include all actuals and both native brackets.

| Existing quiet one-CPU control | Lokad µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: |
| AsyncNonSeekable short | 198,211.92 | 1.632 / 1.933 | 29,689,080 | 183,812 |
| AsyncNonSeekable independent | 197,779.35 | 1.622 / 1.641 | 29,689,080 | 183,812 |
| DiagramLossless8 short | 11,206.35 | 1.167 / 2.078 | 27,419,167 | 7,234 |
| DiagramLossless8 independent | 11,123.58 | 1.166 / 1.204 | 27,419,167 | 7,234 |
| PhotographLossless8 short | 134,828.31 | 1.625 / 1.945 | 20,410,208 | 160,032 |
| Asymmetric RGBA8 short | 191,999.87 | 1.584 / 2.263 | 29,688,696 | 183,812 |
| Asymmetric RGBA16 short | 193,837.99 | 1.600 / 1.847 | 29,688,696 | 183,812 |
| VP8 plus alpha short | 73,450.84 | 1.377 / 1.977 | 21,353,192 | 98,590 |

The original full histogram slab and its copy are removed. One working slab
holds merge populations; two compact pixel-row/tile-column indexes retain first
token index and absolute pixel position. Final remap reconstructs one original
tile in a non-live working slot, preserving live populations. If all slots remain
live there was no merge, and working already contains the originals. Output
populations are accumulated immediately, in original tile order.

[Spatial prefix-code selection](https://www.rfc-editor.org/rfc/rfc9649.html#section-3.7.2.2)
is unchanged. Token starts are monotone; each row cell has one contiguous start
run, including copies skipping rows/tiles. Every original population, merge/map
score, tie and packet stays exact. The two indexes need at most 1024 words/tile,
below the removed 1088+-word slab; no separate histogram scratch is allocated.

Actual capacities are admitted before allocation, including metadata and output
owner overlap. Borrowed tokens do not escape Build. Temporary storage is released
after independent retained ownership is established. Direct cancellation,
terminal refusal, async lifetimes and caller ownership remain. No new governor,
cache, ceiling lease, work quota or fallback. Existing normalization, fresh output
acquisition/delivery and release stay inside equivalent single-CPU boundaries.

Both systems pass 4,957 Release checks (1,975 internal, 2,035 public, 947 live), 636
scalar WebP and 59 initial focused checks. Seven new cases cover tight 24,208 B
aggregate admission/refusal/recovery, clipped/cross-row token conservation,
independent outputs after clearing borrowed tokens, and four separate live slots
without replay. Three native maps/every final population and complete packet
oracles remain. The SAME actual private 0.2.0 package plus three usage examples
passes both systems; published 0.1.0 remains unchanged.

Private indexing proofs cover 664 complete partitions, 3,381,315 token starts,
1,120,784 empty row cells and 1,992 working-slot layouts.
[Evidence](warm-vp8l-indexed-histogram-replay-evidence.json) retains samples, guards,
actual producers/package and the mirrored/member-verified raw master. One
production build per OS is reused; test-only edits require linked test builds,
never another producer. Workers/harnesses/selectors are unchanged. Other affected
costs are historical; opaque VP8, other codecs/decoders/editing/text keep valid
evidence. Stop slab/index/replay variants. Full 255-path warm upper ≤1× and
replacement readiness remain open.
