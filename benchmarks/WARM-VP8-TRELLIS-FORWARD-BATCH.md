# Existing forward batching during VP8 trellis

[Trellis state simplification](WARM-VP8-TRELLIS-STATE-PAIR.md) supersedes the
effort-6 cost below. Effort-4 costs remain valid because that routine is unchanged.

The effort-6 diagram independently measures 26.41 ms: 2.700× reference time
(upper 2.721×), 854,968 B/op and an exact 6,818-byte packet. Elapsed time is 4.12%
lower than the preceding quiet short; this is not a paired independent-baseline
gain claim. Full warm parity remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DiagramLossyTrellis8 short | 26,427.40 | 9,791.90 | 2.699 / 2.849 | 854,968 | 6,818 |
| DiagramLossyTrellis8 confirm | 26,411.47 | 9,783.51 | 2.700 / 2.721 | 854,968 | 6,818 |
| PhotographLossy8 short | 24,637.83 | 9,694.54 | 2.541 / 2.675 | 909,696 | 17,688 |
| DiagramLossy8 short | 14,498.91 | 5,832.30 | 2.486 / 2.694 | 841,608 | 7,100 |
| vp8 short | 84,335.21 | 53,244.41 | 1.584 / 1.792 | 23,358,400 | 98,590 |

Both idle-v2 guards pass for every row. Trellis retains all twelve actual and
filtered confirmation samples. Effort-4 rows are current shorts; older affected
timings are historical. Complete input, normalization and fresh output ownership
and release boundaries retain the frozen reference settings. No ToArray,
preparation, startup, transport or validation is timed. Complete packets and
registered reconstructed pixels remain exact, including normal/scalar decoding.

Uncancelable trellis search reuses the existing four-prediction forward transform.
One batching predicate changes; per-mode coefficient trellis, prices, ties and
reconstruction stay ordered. Uniform-score reuse remains disabled for trellis.
Live tokens and unsupported hardware retain the per-mode fallback. No new kernel,
cache, allocation or owner is introduced: the existing 576-byte batch scratch fits
the admitted 16 KiB. Independent coefficient slices preserve mutation ordering.

Both operating systems pass 4,937 Release checks (1,955 internal, 2,035 public,
947 live), 616 scalar WebP checks and 63 focused forward/trellis/decision checks.
Public traversal controls cover frozen 6,818/85,796-byte effort-6 packets,
live-token fallback, shared-root release, borrowed sources and surviving outputs.
Decoder, editing, admission and cancellation coverage passes. The same actual
private 0.2.0 package and all three examples pass both systems; published 0.1.0
remains unchanged.

Production built once per system and stayed immutable. Existing harnesses, native
workers and short selectors were reused. Only the missing confirm selector was
prepared; its binary-reference project override preserved the selected core.
Preparation timings are unscored. Unaffected codec, decoder, editing and text
evidence remains valid. [Evidence](warm-vp8-trellis-forward-batch-evidence.json)
retains identities, samples, qualification and the mirrored, member-verified raw
archive. This batching round is closed. Next inspect trellis DP setup and repeated
span/index work on the same fixed control.
