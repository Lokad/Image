# Exact WebP cache-history reuse

Existing 513×259 RGBA8 VP8 Q75/method4 retains exact compressed alpha and the
raw-exact 98,590 B packet. Complete operations include normalization, encoding
and fresh independent owned output acquisition/release. Inputs, reference settings
and quality/alpha gates are unchanged. One CPU; noncancellable timing controls.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| VP8 short | 129,032.75 | 53,776.44 | 2.399/2.657 | 24,329,736 |
| VP8 independent | 129,327.57 | 53,621.51 | 2.412/2.430 | 24,329,736 |
| VP8L current short | 296,245.81 | 121,310.53 | 2.442/3.019 | 48,184,648 |

Independent VP8 confirmation uses 3.6% less elapsed than the
[preceding confirmation](WARM-VP8-VECTOR-RESIDUAL.md). All 12 actual iterations
are retained and selected; both idle-v2 guards pass. Allocation rises 248,784 B/op
(1.03%). The affected VP8L control has a current short reconciliation and the same
exact 183,812 B packet, with 124,392 B/op extra allocation. Its timing uncertainty
does not support an independent lossless speedup claim.

One derived byte per private working pixel records the smallest cache width hit.
Both read-only seed streams reuse it, avoiding repeated per-width pixel walks.
Histograms, tie choices and logical work remain exact. Full history/cache/count
capacity plus coarse metadata is admitted before allocation and released after
both scores. No source copy or retained cache is added. Single-seed, cancellable,
zero-width and tight local/work cases retain their individual checks. Shared
refusal is terminal; eligibility never uses shared available-capacity snapshots.

Both platforms pass 5,115 checks each, including 947 live comparisons, 15 scalar
history/parsing checks and 56 scalar public VP8/VP8L packet checks. The independent
arithmetic proof covers 2,048 seed pairs, 24,558 full histograms and every prefix
cache hit. Aggregate admission, preallocation peaks, terminal refusal, cleanup,
recovery and existing cancellation paths pass. The SAME actual private 0.2.0
Release package and all three consumer examples pass both platforms.

[Evidence](warm-vp8-cache-history-evidence.json) records samples, ownership bounds,
qualification and precise invalidation. JPEG, PNG, decoding, editing and text
reuse unchanged evidence. No controls were added. Full 257-case warm parity and
replacement readiness remain open.
