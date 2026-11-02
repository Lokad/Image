# Fused prepared VP8L counting

The accepted BCR79 stage saves **5.43 ms / 4.53% elapsed** on the independent
photograph pair, with separate raw confidence intervals in both filtered and
all-actual views. The native-normalized point gain is 5.78%; **normalized confidence
still overlaps**. This is a retained architectural gain, not parity. No additional
sampling was used to obtain a favorable result.

| Complete encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Photograph before | 119,992.38 | 90,429.21 | 1.327 / 1.364 | 19,329,200 |
| Photograph after | 114,562.59 | 91,634.97 | 1.250 / 1.280 | 19,329,232 |
| Text/diagram after | 9,998.02 | 10,149.38 | 0.985 / 1.014 | 10,483,979 |
| Transparent lossy with lossless alpha after | 74,920.64 | 59,428.00 | 1.261 / 1.290 | 19,226,096 |

Each row has twelve actuals and twelve native samples before/after on the quiet
single-CPU VM. Lossless uses labeled libwebp exact=1; transparent lossy uses the
existing FFmpeg reference. Inputs/settings, complete operations, normalization,
fresh output ownership/disposal and original quality checks are unchanged.
Packets remain exactly 160,032 / 7,234 / 98,590 bytes. The diagram confidence bound
is open; photograph and transparent lossy retain confirmed CPU gaps. Only the
photograph has an independent before/after gain; other gain screens are diagnostic.

Prepared size comparisons previously emitted token bodies into a discard stream.
Weighted symbol bits now come from the existing canonical-code construction loop,
plus fixed length/distance extras. One body-bit scalar per image avoids both the
discard emission and the earlier experiment's additional population/length scans.
Real emission, headers, auxiliary images, tree bits and pending-bit boundaries keep
their original paths. Two histogram-add loops share safe SIMD/scalar addition;
compact logical/physical mapping, merge order and ties are unchanged. See
[RFC 9649](https://www.rfc-editor.org/rfc/rfc9649.html#section-3.6.2.2) for the format.

The extra scalar costs 8–40 B/encode across the four direct controls. Admitted
peaks are unchanged: photograph12,217,052 B, diagram6,174,402 B, alpha6,579,383 B.
These ownership charges exclude borrowed inputs/caller stream capacity and are
not physical heap peaks. An original 127×65 bar control also preserves its full
packet, source and cleanup. No cross-operation pool or new admission model is added.

Windows/Linux each pass 5,401 full/live checks and 224 affected checks in normal
and scalar modes. The same actual Release package and all three usage examples
pass both consumers, including shared refusal/cancellation and asynchronous
surviving outputs. Published packages are unchanged; publication is postponed.

The former [counting](VP8L-PREPARED-BODY-COUNT-REJECTED.md) and
[addition](VP8L-HISTOGRAM-ADDITION-UNQUALIFIED.md) trials remain history, not current
proof or permanent prohibitions. Their repeated scans/old thresholds did not
authorize this acceptance. The revised stage is measured as one coherent change;
its gain is not attributed separately to either component.

[Evidence](vp8l-fused-prepared-counting-evidence.json) preserves complete actuals,
confidence caveats, source/producer pins and exact outputs. Raw
vp8l-fused-prepared-counting.tar.xz: 12,571,624 B / 523 members,
SHA256 e0bc67a8a964a5bc96894211d69956d6ed95bbc106def3380bcc8b53ac5c9139,
mirrored/member-verified, including all scripts, reused runtimes, guards, original/
selected sources/producers and old/new package artifacts. Unchanged evidence is reused.
