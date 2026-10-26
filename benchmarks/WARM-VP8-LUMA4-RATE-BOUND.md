# Exact VP8 luma rate-cost pruning

The existing 513×259 RGBA8 VP8 Q75/method4 control retains complete normalization,
encoding, fresh independent output acquisition/release and one CPU. Alpha and
complete packets remain exact. No image, effort or settings grid was added.

| Quiet run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| short | 92,622.86 | 53,578.73 | 1.729 / 1.871 | 23,358,400 | 98,590 |
| confirm | 92,818.18 | 53,494.23 | 1.735 / 1.744 | 23,358,400 | 98,590 |

Independent current cost is 1.735×/upper 1.744. Elapsed is 2.0% lower than the
[preceding quiet short](WARM-VP8L-CHAIN-CAPACITY.md), with unchanged allocation.
This is not a paired independent-baseline gain claim. All 12 actuals and both
idle-v2 guards pass; conservative bounds include unfiltered results and both
native brackets. Warm 1× remains open on this declared content/settings scope.

The candidate score is `(R+H)*lambda + 256*(D+S)`. On valid plans, distortion and
texture are nonnegative, so rate/header alone is a lower bound. Calculate pure
rate cost after quantization; if that bound ties/exceeds the incumbent, skip
reconstruction and texture. Other candidates compute the same full metric.
The original strict tie order, chosen scores/coefficients/pixels and clipping,
diffusion, entropy statistics and output ownership remain unchanged. Int64 rate
arithmetic keeps the original bounds; no floating-point approximation is added.

One diagnostic encode preserves the packet and identifies 28,891 of 89,760
candidates (32.2%) eligible for this bound. Header alone covers only 1.8% and is
not adopted. These counts are control-specific, not CPU attribution or a general
skip-rate claim. Calling rate first adds work for the old 4.1% partial-score
prunes; complete-operation measurements include that cost.

Both OS pass 4,911 Release checks each, including 947 live FFmpeg comparisons,
plus 597 affected scalar checks each. Existing native frozen opaque/diagram and
method4/5/6 decision traces retain all score/state/output fields. Alias,
cancellation, resource and complete codec/editing tests pass. The SAME actual
private 0.2.0 package and all three usage examples pass both OS; published 0.1.0 is
unchanged. No buffer, owner, admission or cancellation checkpoint is added/removed.

One immutable core serves the bounded short/independent round and Linux
qualification; native/benchmark/generated workers and unaffected evidence are
reused. VP8L matching, JPEG/PNG, decoders/editing/text are unchanged. Older affected
VP8 costs remain historical. [Evidence](warm-vp8-luma4-rate-bound-evidence.json)
records raw results, diagnostics, producer/package identities and precise reuse.
Stop this bound round; full 253-path parity and replacement readiness remain open.
