# Exact VP8 texture lower bound

The frozen photograph independently uses **4.3% less elapsed**:14.373ms versus
a fresh15.013ms baseline, with unchanged809072B/op and exact17688B packets.
Its **1.479× / upper1.483** CPU gap remains.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Baseline photograph, short | 15,167.52 | 9,717.97 | 1.561 / 1.600 | 809,072 | 17,688 |
| Baseline photograph, confirm | 15,013.01 | 9,720.07 | 1.545 / 1.552 | 809,072 | 17,688 |
| Bounded photograph, short | 14,280.82 | 9,717.46 | 1.470 / 1.508 | 809,072 | 17,688 |
| Bounded photograph, confirm | 14,372.74 | 9,719.09 | 1.479 / 1.483 | 809,072 | 17,688 |
| DiagramLossy8, short | 8,186.76 | 5,828.66 | 1.405 / 1.440 | 741,137 | 7,100 |
| DiagramLossyTrellis8, short | 14,921.39 | 9,825.29 | 1.519 / 1.614 | 754,344 | 6,818 |
| Alpha8, short | 70,132.45 | 53,670.66 | 1.307 / 1.393 | 21,353,208 | 98,590 |
| Opaque16, short | 20,736.03 | 15,578.69 | 1.331 / 1.368 | 1,201,552 | 72,380 |

Existing513×259 inputs/settings and complete normalization, encoding and fresh
independent output acquisition/release remain. The short5.8% gain meets the
predeclared5% screen gate; independent4.3% is smaller, with separated filtered
and unfiltered confidence intervals. These are separate sequential means, not
paired statistics or general-content gains. Baseline retains12actual/11filtered,
candidate12/12. Native brackets and all idle-v2 guards pass. Four companions are
current short screens, without independent gain/parity claims. Native allocation
is unmeasured. No content/settings grid or quality search is added.

An untimed own-source replay finds29735of47869remaining texture scores are
already losing from rate/header plus reconstructed SSD:62.12%. It still computes
full scores and asserts each rejection, retaining the frozen input and packet.
The production change skips only these losing texture evaluations after existing
rate pruning, reconstruction and SSD. Texture is nonnegative, so ties cannot
improve; first-candidate initialization, strict ties, native score/state/pixels,
uniform reuse and per-mode cancellation remain. No new storage or ownership.

Both systems pass5052Release checks each, including947live comparisons,21internal/
27public scalar checks, the SAME actual private0.2.1 package and three examples.
Existing native RDO/trellis score/rolling-state traces, frozen method4/6 packets,
aliases, cancellation and memory/async cleanup coverage exercise the production
encoder. One successful immutable core build perOS and cached harness/native/
selectors serve the bounded experiment; full/package qualification runs once
after selection. A companion manifest-prefix error stopped before timing and
was corrected without repeating completed observations. Published releases remain.

Method3/4/5/6 Luma4 decision costs change; five displayed controls are refreshed.
Other affected named callers need applicable current costs before pass claims.
Method0/1/2 remains unchanged and keeps the prior effort2 evidence. Current alpha
allocation also captures the prior predictor Analysis object's added reference
field; green-only pricing and its admitted capacity remain unchanged. All VP8L,
JPEG/PNG, decoding, editing/text and relevant unchanged confirmations remain.

[Numeric evidence](warm-vp8-texture-distortion-bound-evidence.json) retains all
samples, identities, raw count evidence, qualification and the mirrored/member-
verified raw master. Stop this primitive; the full267-path warm single-CPU ≤1×
objective remains open.
