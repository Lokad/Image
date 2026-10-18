# Bounded VP8 coefficient adaptation

Adapting coefficient probabilities from the first at most 64 macroblocks cuts
the original opaque packet **333,580→151,606 bytes (54.5%)** and allocation about
**41.6%**. Complete encoding improves **7.9% / 9.8%** for RGBA8/16, remaining
**1.320× / 1.323×** FFmpeg. Alpha-bearing packets shrink **39.2%** and encode
about **8.5–8.8%** faster. These are complete-operation measurements, including
normalization, admission, fresh output growth and ownership.

Photograph/diagram packets shrink **21,826→21,432 (1.8%)** and **8,958→8,288
(7.5%)** at the same frozen quality settings and decoded pixels. Their complete
times increase **1.6% / 1.7%**; photo allocation increases **3.2%**, diagram
allocation decreases **0.2%**. The diagram's independent mean is **1.000208×**,
conservative upper **1.009341×**: its earlier passing timing is superseded and
parity remains open. Other affected passing cases, including the new budget
fallback, have independent upper≤1 evidence. Full uniform parity remains open.

| Complete VP8 encode | Managed / reference µs | Ratio / upper | Managed allocated B/op | Managed / reference bytes | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Photograph RGBA8 | 7,643.09 / 9,722.18 | 0.786 / 0.790 | 558,520 | 21,432 / 17,688 | Independent parity |
| Text/diagram RGBA8 | 5,842.06 / 5,840.84 | 1.000 / 1.009 | 489,932 | 8,288 / 7,100 | Independent gap |
| Original alpha RGBA8 | 20,670.47 / 53,317.03 | 0.388 / 0.391 | 2,109,290 | 286,532 / 98,590 | Independent parity |
| Original alpha RGBA16 | 21,352.42 / 53,437.67 | 0.400 / 0.403 | 2,108,982 | 286,532 / 98,590 | Independent parity |
| Original opaque RGBA8 | 20,420.05 / 15,471.74 | 1.320 / 1.446 | 1,482,092 | 151,606 / 72,380 | Short gap |
| Original opaque RGBA16 | 20,641.94 / 15,607.33 | 1.323 / 1.375 | 1,481,928 | 151,606 / 72,380 | Short gap |
| Original alpha RGBA8, work budget 32M | 22,866.92 / 53,563.20 | 0.427 / 0.430 | 2,616,016 | 471,436 / 98,590 | Independent parity |

The independently implemented encoder gathers branch counts while coding a
bounded prefix. An empirical model is selected only when its estimated bit
savings cover probability-update signaling. Private converted sample bytes
retain low coefficient magnitudes; two side bits preserve sign and the ninth
magnitude bit. A vertex-bound test covers every residual-box extreme and proves
magnitude≤510. Prefix emission reconstructs its independent entropy contexts;
later blocks emit directly. Source pixels, prediction, DCT/quantization and
reconstruction run once. No full coefficient frame, second DCT, event tape,
upstream implementation or input copy is introduced.

Optional conservative memory/work/header admission retains the original fixed
model when declined. `WarmWebp/EncodeBudget8` uses the existing alpha raster and
MaxWork=32,000,000: the complete encoder consumes31,582,932 units and reproduces
the old471,436-byte packet exactly. This is one distinct caller-budget algorithm
boundary, with no new image or settings grid. Cancellation, structured failures,
shared admission, source ownership and surviving outputs are tested. Auxiliary
state is at most24,064 bytes; tighter limits add no mandatory reservation.

All six default packets change, while their managed/normal/scalar native decoded
pixels and frozen source/alpha/decoder metrics remain identical. Native packets,
workers, quality settings and effort4 are reused. Original controls use managed100/
native75, photograph78/75, diagram75/75; PSNR gaps remain within0.25dB. Equal
numeric qualities are not assumed equivalent. The quality catalog labels other
selection packet sizes/hashes as historical; no resampling or retuning occurred.

Seven three-actual screens and five independent twelve-actual confirmations
retain all81 actuals/outliers, native pre/post50ms brackets, idle-v2 guards and
exact child identities. The diagram received one confirmation because its short
mean was only1.0007×, not a demonstrated material gap. No repeat was run on the
clear opaque gaps. A rejected full-frame alternative is retained separately:
its149,570-byte packet was smaller but23.30ms opaque16 time was1.8% slower.
The original completed prefix opaque16 screen is reused after the harness's
policy-label update; its timed public operation and core bytes are unchanged.
Two postprocessing corrections reuse completed logs/guards, without timed reruns:
case-insensitive serialized quality field names and selecting the prefix result
rather than the rejected full-cache result from an inherited round list.

All **4,167 tests/OS** (1,414 internal,1,825 public,928 liveFFmpeg), **183 affected
intrinsics-disabled checks/OS** (149public,34internal), and the **same actual
Windows private package** consumer/all three examples pass on Windows/Linux.
Only six existing VP8 timings and their packet hashes/sizes are invalidated;
the named budget boundary is added. Lossless encoding, decoding, other codecs,
inspection, edits, drawing and text retain applicable evidence. Stop passing
cases until affected. Production stays safe managed dependency-free .NET10.

Linux producer8b87655188e2a03679cd835b336067d0903d50ffc6c41c50e25f3988e99a33e1;
Windows9e72e960af561bfaa1ec126307b22ac5dc90a428f44e8f41d47c281552ffac8d.
Private package08f20e17aeded43dbe0991ad134c438196d3a01fe8c98f2597f88806f281758e.
The prefix screen uses FF58 harness; later screens/confirmations use da41b879...
with the same immutable core and prepared timing drivers. Source67b82d... for
the prefix screen,0a5ce716... for opaque8,9d457d... for subsequent timings;
differences are harness/catalog policy and frozen packet metadata. The later
SPEC clarification changes no production path or measured core.

[Numeric evidence](warm-vp8-adaptive-probabilities-evidence.json) preserves all
statistics, guards, allocations, packets, boundaries and identities. Raw on both
hosts: `h9h-measurements.tar.gz`
a2eaadb2df20b87b4787998558d9c8c21721a177b4a75f916e4b84a3150248f1;
`h9h-qualified-windows.tar.gz`
286229d45bfd489a1716818d077fcd13c64fc6d6919ebe0a4548cd1a5061a767;
`h9h-qualified-linux.tar.gz`
43641d6ca2eb629054b1513e7165c2b5600c56bc40ddfa61386ac64edddd0426.
The published0.1.0 package/tag remains unchanged.
