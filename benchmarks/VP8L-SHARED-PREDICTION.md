# Shared VP8L prediction preparation

Sharing neighbor loads and channel arithmetic across all fourteen full-color
predictors saves **7.29 ms / 6.42%** on the independent photograph pair.
Filtered and all-actual raw confidence intervals separate; conservative
native-normalized confidence also separates. This is an incremental gain;
photograph and transparent-alpha CPU parity remain open.

| Complete encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Photograph before | 113,546.65 | 92,750.51 | 1.224 / 1.256 | 19,329,232 |
| Photograph after | 106,259.96 | 91,719.77 | 1.159 / 1.183 | 19,329,130 |
| Text/diagram after | 10,031.92 | 10,313.08 | 0.973 / 1.014 | 10,483,979 |
| Transparent lossy with lossless alpha after | 75,554.89 | 58,496.32 | 1.292 / 1.314 | 19,226,096 |

Twelve actuals and twelve native samples before/after each run use the quiet VM,
one CPU and unchanged complete-operation boundaries. Lossless compares labeled
libwebp exact=1; transparent lossy compares FFmpeg. Diagram parity confidence is
open. Only the photograph has a fresh independent before/after pair; the other
rows refresh costs, without an incremental-gain claim. Native photograph time
drifted −1.11%; normalized point gain is 5.37%.

The selector already traversed each pixel once. The change removes its fourteen
generic predictor dispatches, repeated neighbor loads and duplicated channel
averages. Packed floor averages and one shared channel pass preserve selection
ties, clamping and signed half-difference rounding from
[RFC 9649](https://www.rfc-editor.org/rfc/rfc9649.html#section-3.5.1).
Recursive tile order, entropy, history, modes, residuals and candidate decisions
are unchanged. The useful green-only builder and portable execution remain.

No allocation, cache or admission boundary is added. Four direct controls keep
identical allocations and admitted peaks: photograph 12,217,052 B, diagram
6,174,402 B, alpha 6,579,383 B and original 127×65 bars 312,716 B. Charges exclude
borrowed input and caller stream capacity; they are not physical heap peaks.
The small photograph B/op difference in the table is measurement accounting,
not a storage reduction. Full packets remain 160,032 / 7,234 / 98,590 / 660 B.
The short screen's small companion slowdowns remain in the evidence.

Windows/Linux each pass 5,404 full/live checks and 227 affected checks in normal
and scalar modes. The same actual Release package and three usage examples pass
both consumers, including refusal, cancellation and independent asynchronous
outputs. Published releases are unchanged; publication is postponed.

The preceding shared-prefix/run experiment was **inconclusive and restored**:
photograph/diagram point gains were 2.13%/0.77%, alpha was 1.15% slower, and raw
and normalized confidence overlapped. Native drift changed the normalized
estimates. No favorable resampling or merge followed. Its source, immutable
producers and all results remain in webp-shared-prefix-inconclusive.tar.xz
(665,304 B; SHA256
06dcfffdea8755f71cfb32dad1a09da4717b962e28cd5bb6d2231fed68fb4680),
mirrored/member-verified; it depends on the retained
[counting-stage](VP8L-FUSED-PREPARED-COUNTING.md) producer archive.

[Current evidence](vp8l-shared-prediction-evidence.json) records source/producer
pins, all actuals, guards, exact packets, qualification and raw reproduction.
Raw vp8l-shared-prediction.tar.xz: 12,717,688 B / 504 members, SHA256
898c3af802549269ef5f16120f68e7166046582596edf8eeceb85d2352cc77ee,
mirrored/member-verified, including immutable runtimes and package artifacts.
Older affected encoder costs remain explicitly historical; unchanged paths reuse
valid evidence. No new benchmark harness, worker or selector was built.
