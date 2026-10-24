# Exact WebP common-count entropy costs

Existing 513×259 RGBA8 VP8L retains method4/Q75 and the approved libwebp
reference with `exact=1`. Complete operations include normalization, encoding and
fresh independently owned output acquisition/release. One CPU. Hidden RGB/alpha
and the raw-exact 183,812 B packet remain. The affected VP8 control retains its
Q75/method4 settings, exact compressed alpha and raw-exact 98,590 B packet.

| Quiet VM run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| VP8L short | 219,207.13 | 121,681.64 | 1.801/2.280 | 48,184,648 |
| VP8L independent | 218,554.41 | 121,565.25 | 1.798/1.829 | 48,184,648 |
| VP8 current short | 125,599.61 | 53,530.57 | 2.346/2.465 | 24,329,736 |

Independent VP8L confirmation uses 26.5% less elapsed than the
[preceding confirmation](WARM-VP8L-CACHE-HISTORY.md), with unchanged allocation.
All 12 actual iterations are retained, 11 selected; both contribute to the
conservative upper. Both idle-v2 guards pass. VP8 is a current short reconciliation,
without an independent speedup claim. Inputs/reference settings are unchanged.

The common zero/small-count entropy path is inlined; existing large-count
arithmetic stays in a separate helper. Every Q23 value, formula, rounding,
threshold and decision remains. No storage, scope, table or copy is added.
Work, cancellation, shared/local admission, refusal and output ownership remain.
Targeted instruction mapping of the retained trace identified entropy call sites;
parent stack labels alone did not establish predictor or loop costs.

Both platforms pass 5,119 checks each, including 947 live comparisons, 11 scalar
entropy/green/color checks and 56 scalar public VP8/VP8L checks. Existing exhaustive
count-domain, frozen population/cost, native map/token/packet and exact allocation
gates pass. The SAME actual private 0.2.0 Release package and all three examples
pass both platforms. No new image/settings controls or extra benchmark builds.

[Evidence](warm-vp8l-common-entropy-evidence.json) records samples, identities and
qualification. Other affected WebP timings remain historical. JPEG, PNG, decoding,
editing and text reuse unchanged evidence. Larger JPEG/VP8 gaps take priority;
full 257-case warm parity and replacement readiness remain open.
